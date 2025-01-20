namespace Novibet.Interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<CountryReport>> GetCountryReportAsync(string? twoLetterCodes);
    }

}

