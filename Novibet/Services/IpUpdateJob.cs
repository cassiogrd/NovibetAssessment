using Newtonsoft.Json;
using Novibet.Models;
using Quartz;
using Serilog;
using StackExchange.Redis;

public class IpUpdateJob : IJob
{
    private readonly IpInfoRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly IConnectionMultiplexer _redis;

    // recebe 3 depndencias
    public IpUpdateJob(IpInfoRepository repository, HttpClient httpClient, IConnectionMultiplexer redis)
    {
        _repository = repository;// Para acessar o banco de dados
        _httpClient = httpClient;// Para Consultar API externa
        _redis = redis;// Gerenciar cache Redis
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Executando o Job de atualização de IPs...");

        int batchSize = 100;
        int offset = 0;

        while (true)
        {
            var ipsBatch = await _repository.GetIpsBatchAsync(batchSize, offset);
            if (!ipsBatch.Any()) break;

            foreach (var ipRecord in ipsBatch) // para cada ip, itera
            {
                try
                {
                    Log.Information("Atualizando informações do IP: {Ip}", ipRecord.IP);

                    // chama a api para obter informações
                    var newInfo = await GetIpInfoFromExternalApiAsync(ipRecord.IP);
                    if (newInfo == null) continue;

                    // se os codigos do país mudou, alterar
                    if (ipRecord.Country.TwoLetterCode != newInfo.TwoLetterCode ||
                        ipRecord.Country.ThreeLetterCode != newInfo.ThreeLetterCode)
                    {
                        //verifica se o banco contem o país correspondente ao codigo retornado da API
                        var country = await _repository.GetCountryByCodeAsync(newInfo.TwoLetterCode);
                        if (country == null) continue;

                        // Atualiza no banco de dados
                        ipRecord.CountryId = country.Id;
                        ipRecord.UpdatedAt = DateTime.UtcNow;
                        await _repository.UpdateIpInfoAsync(ipRecord);


                        // 2. Verificar se o dado existe no cache
                        var cacheKey = $"ipinfo:{ipRecord.IP}";
                        var cacheExists = await _redis.GetDatabase().KeyExistsAsync(cacheKey);

                        if (cacheExists)
                        {
                            // 3. Invalida o cache se o dado já existir
                            await _redis.GetDatabase().KeyDeleteAsync(cacheKey);
                            Log.Information("Cache invalidado para o IP: {Ip}", ipRecord.IP);
                        }

                        // 4. Adicionar o dado atualizado ao cache
                        var updatedData = JsonConvert.SerializeObject(new
                        {
                            CountryName = newInfo.CountryName,
                            TwoLetterCode = newInfo.TwoLetterCode,
                            ThreeLetterCode = newInfo.ThreeLetterCode
                        });

                        await _redis.GetDatabase().StringSetAsync(cacheKey, updatedData);
                        Log.Information("Cache atualizado para o IP: {Ip}", ipRecord.IP);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro ao atualizar informações do IP: {Ip}", ipRecord.IP);
                }
            }

            offset += batchSize;
        }

        Log.Information("Job de atualização de informações de IPs concluído.");
    }

    private async Task<IpInfoResponse?> GetIpInfoFromExternalApiAsync(string ip)
    {
        try
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
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao chamar a API externa para o IP: {Ip}", ip);
            return null;
        }
    }
}
