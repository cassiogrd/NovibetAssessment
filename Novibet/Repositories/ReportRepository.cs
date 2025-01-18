using Dapper;
using Microsoft.Data.SqlClient;

public class ReportRepository
{
    private readonly AdoConnectionService _adoConnectionService;

    public ReportRepository(AdoConnectionService adoConnectionService)
    {
        _adoConnectionService = adoConnectionService;
    }

    public async Task<IEnumerable<CountryReport>> GetCountryReportAsync(string? twoLetterCodes)
    {
        using var connection = _adoConnectionService.CreateConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT 
                c.Name AS CountryName,
                COUNT(ip.Id) AS AddressesCount,
                MAX(ip.UpdatedAt) AS LastAddressUpdated
            FROM 
                Countries c
            LEFT JOIN 
                IPAddresses ip ON c.Id = ip.CountryId
            WHERE 
                (@TwoLetterCodes IS NULL OR c.TwoLetterCode IN (SELECT Value FROM STRING_SPLIT(@TwoLetterCodes, ',')))
            GROUP BY 
                c.Name
            ORDER BY 
                c.Name;
        ";

        return await connection.QueryAsync<CountryReport>(sql, new { TwoLetterCodes = twoLetterCodes });
    }
}
