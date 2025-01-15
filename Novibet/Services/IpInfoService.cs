using Microsoft.EntityFrameworkCore;
using System.Net.Http;

public class IpInfoService
{
    private readonly HttpClient _httpClient;
    private readonly IpInfoRepository _repository;

    // Construtor: Recebe o repositório como dependência
    public IpInfoService(IpInfoRepository repository, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _repository = repository;
    }

    // Método principal para buscar informações de um IP
    public async Task<IPAddress?> GetIpInfoAsync(string ip)
    {
        // 1️ - Buscar no banco de dados
        var ipData = await _repository.GetIpInfoFromDbAsync(ip);
        if (ipData != null)
        {
            return ipData; // Retorna o dado do banco
        }

        // 2 -  Caso não exista, chamar a API externa
        var externalData = await GetIpInfoFromExternalApiAsync(ip);
        if (externalData != null)
        {
            // Buscar o CountryId no banco com base no código do país
            var country = await _repository.GetCountryByCodeAsync(externalData.Country.TwoLetterCode);
            if (country == null)
            {
                throw new Exception($"Country not found for code {externalData.Country.TwoLetterCode}");
            }

            // Salvar no banco para uso futuro
            await _repository.SaveIpInfoAsync(new IPAddress
            {
                CountryId = externalData.CountryId,
                IP = ip,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Retorna o dado obtido
            return externalData;
        }

        // Lançar exceção caso o IP não seja encontrado
        throw new Exception("IP not found");
    }

    // Simular chamada para API externa (pode ser ajustado para a integração real)
    private async Task<IPAddress?> GetIpInfoFromExternalApiAsync(string ip)
    {
        //faz a requisição HTTP
        var response = await _httpClient.GetStringAsync($"https://ip2c.org/{ip}");

        // Verifica a resposta Exemplo de resposta: "1;US;USA;United States"
        if (!string.IsNullOrEmpty(response) && response.StartsWith("1;"))
        {
            //divide os dados
            var parts = response.Split(';');
            if (parts.Length >= 4)
            {
                return new IPAddress
                {
                    IP = ip,
                    Country = new Country
                    {
                        TwoLetterCode = parts[1],
                        ThreeLetterCode = parts[2],
                        Name = parts[3],
                        CreatedAt = DateTime.UtcNow
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
        }

        return null; // IP não encontrado ou resposta inválida
    }

    

}
