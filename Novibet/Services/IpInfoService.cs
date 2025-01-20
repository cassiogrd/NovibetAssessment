using Novibet.Models; // Importa IpInfoResponse
using Newtonsoft.Json;
using StackExchange.Redis;
using Serilog;
using Novibet.Interfaces;

public class IpInfoService
{
    private readonly HttpClient _httpClient;
    private readonly IIpInfoRepository _repository;
    private readonly IConnectionMultiplexer _redis;

    public IpInfoService(IIpInfoRepository repository, HttpClient httpClient, IConnectionMultiplexer redis)
    {
        _httpClient = httpClient;
        _repository = repository;
        _redis = redis;
    }

    public async Task<IpInfoResponse?> GetIpInfoAsync(string ip)
    {
        Log.Information("Iniciando busca de informações para o IP: {Ip}", ip);

        try
        {
            // 1. Buscar no cache
            var cacheKey = $"ipinfo:{ip}";
            var cachedData = await _redis.GetDatabase().StringGetAsync(cacheKey);
            if (cachedData.HasValue)
            {
                Log.Information("Dados do IP {Ip} encontrados no cache.", ip);
                return JsonConvert.DeserializeObject<IpInfoResponse>(cachedData);
            }

            Log.Information("Dados do IP {Ip} não encontrados no cache. Buscando no banco de dados.", ip);

            // 2. Buscar no banco de dados
            var dbData = await _repository.GetIpInfoFromDbAsync(ip);
            if (dbData != null)
            {
                Log.Information("Dados do IP {Ip} encontrados no banco de dados.", ip);
                var resultFromDb = new IpInfoResponse
                {
                    CountryName = dbData.Country.Name,
                    TwoLetterCode = dbData.Country.TwoLetterCode,
                    ThreeLetterCode = dbData.Country.ThreeLetterCode
                };

                // Salvar no cache para futuras consultas
                await _redis.GetDatabase().StringSetAsync(cacheKey, JsonConvert.SerializeObject(resultFromDb));
                Log.Information("Dados do IP {Ip} salvos no cache.", ip);
                return resultFromDb;
            }

            Log.Information("Dados do IP {Ip} não encontrados no banco de dados. Buscando na API externa.", ip);

            // 3. Buscar na API externa
            var externalData = await GetIpInfoFromExternalApiAsync(ip);
            if (externalData != null)
            {
                Log.Information("Dados do IP {Ip} encontrados na API externa.", ip);

                // Buscar o CountryId no banco de dados para o país obtido da API
                var country = await _repository.GetCountryByCodeAsync(externalData.TwoLetterCode);
                if (country == null)
                {
                    Log.Error("País não encontrado para o código {Code} retornado pela API.", externalData.TwoLetterCode);
                    throw new Exception($"Country not found for code {externalData.TwoLetterCode}");
                }

                // Salvar no banco de dados
                await _repository.SaveIpInfoAsync(new IPAddress
                {
                    CountryId = country.Id, // O ID vem do banco
                    IP = ip,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                Log.Information("Dados do IP {Ip} salvos no banco de dados.", ip);

                // Criar o resultado final
                var resultFromApi = new IpInfoResponse
                {
                    CountryName = externalData.CountryName,
                    TwoLetterCode = externalData.TwoLetterCode,
                    ThreeLetterCode = externalData.ThreeLetterCode
                };

                // Salvar no cache
                await _redis.GetDatabase().StringSetAsync(cacheKey, JsonConvert.SerializeObject(resultFromApi));
                Log.Information("Dados do IP {Ip} retornados pela API externa salvos no cache.", ip);
                return resultFromApi;
            }

            Log.Warning("Nenhum dado encontrado para o IP: {Ip} em nenhuma das fontes.", ip);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao buscar informações para o IP: {Ip}", ip);
            throw;
        }
    }

    private async Task<IpInfoResponse?> GetIpInfoFromExternalApiAsync(string ip)
    {
        Log.Information("Fazendo chamada para a API externa para o IP: {Ip}", ip);

        try
        {
            var response = await _httpClient.GetStringAsync($"https://ip2c.org/{ip}");
            if (!string.IsNullOrEmpty(response) && response.StartsWith("1;"))
            {
                var parts = response.Split(';');
                if (parts.Length >= 4)
                {
                    Log.Information("Dados retornados com sucesso da API externa para o IP: {Ip}", ip);
                    return new IpInfoResponse
                    {
                        CountryName = parts[3],
                        TwoLetterCode = parts[1],
                        ThreeLetterCode = parts[2]
                    };
                }
            }

            Log.Warning("Resposta inválida da API externa para o IP: {Ip}", ip);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao chamar a API externa para o IP: {Ip}", ip);
            throw;
        }
    }
}
