using Novibet.Models; // Importa IpInfoResponse
using Newtonsoft.Json;
using StackExchange.Redis;

public class IpInfoService
{
    private readonly HttpClient _httpClient;
    private readonly IpInfoRepository _repository;
    private readonly IConnectionMultiplexer _redis;

    public IpInfoService(IpInfoRepository repository, HttpClient httpClient, IConnectionMultiplexer redis)
    {
        _httpClient = httpClient;
        _repository = repository;
        _redis = redis;
    }

    public async Task<IpInfoResponse?> GetIpInfoAsync(string ip)
    {
        var cacheKey = $"ipinfo:{ip}";

        // 1. Buscar no cache
        var cachedData = await _redis.GetDatabase().StringGetAsync(cacheKey);
        if (cachedData.HasValue)
        {
            return JsonConvert.DeserializeObject<IpInfoResponse>(cachedData);
        }

        // 2. Buscar no banco de dados
        var dbData = await _repository.GetIpInfoFromDbAsync(ip);
        if (dbData != null)
        {
            var resultFromDb = new IpInfoResponse
            {
                CountryName = dbData.Country.Name,
                TwoLetterCode = dbData.Country.TwoLetterCode,
                ThreeLetterCode = dbData.Country.ThreeLetterCode
            };

            // Salvar no cache para futuras consultas
            await _redis.GetDatabase().StringSetAsync(cacheKey, JsonConvert.SerializeObject(resultFromDb));
            return resultFromDb;
        }

        // 3. Buscar na API externa
        var externalData = await GetIpInfoFromExternalApiAsync(ip);
        if (externalData != null)
        {
            // Buscar o CountryId no banco de dados para o país obtido da API
            var country = await _repository.GetCountryByCodeAsync(externalData.TwoLetterCode);
            if (country == null)
            {
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

            // Criar o resultado final
            var resultFromApi = new IpInfoResponse
            {
                CountryName = externalData.CountryName,
                TwoLetterCode = externalData.TwoLetterCode,
                ThreeLetterCode = externalData.ThreeLetterCode
            };

            // Salvar no cache
            await _redis.GetDatabase().StringSetAsync(cacheKey, JsonConvert.SerializeObject(resultFromApi));
            return resultFromApi;
        }

        return null;
    }


    private async Task<IpInfoResponse?> GetIpInfoFromExternalApiAsync(string ip)
    {
        var response = await _httpClient.GetStringAsync($"https://ip2c.org/{ip}");
        if (!string.IsNullOrEmpty(response) && response.StartsWith("1;"))
        {
            var parts = response.Split(';');
            if (parts.Length >= 4)
            {
                return new IpInfoResponse
                {
                    CountryName = parts[3],
                    TwoLetterCode = parts[1],
                    ThreeLetterCode = parts[2]
                };
            }
        }

        return null;
    }
}
