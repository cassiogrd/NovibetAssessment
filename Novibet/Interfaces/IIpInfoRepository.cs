namespace Novibet.Interfaces
{
    public interface IIpInfoRepository
    {
        Task<IPAddress?> GetIpInfoFromDbAsync(string ip);
        Task SaveIpInfoAsync(IPAddress ipAddress);
        Task<Country?> GetCountryByCodeAsync(string twoLetterCode);
        Task<List<IPAddress>> GetIpsBatchAsync(int batchSize, int offset);
        Task UpdateIpInfoAsync(IPAddress ipAddress);
    }

}


