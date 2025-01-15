using Microsoft.EntityFrameworkCore;

public class IpInfoRepository
{
    private readonly AppDbContext _context;

    public IpInfoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Buscar informações de um IP no banco
    public async Task<IPAddress?> GetIpInfoFromDbAsync(string ip)
    {
        return await _context.IPAddresses
            .Include(i => i.Country)
            .FirstOrDefaultAsync(i => i.IP == ip);
    }

    // Salvar informações de IP no banco
    public async Task SaveIpInfoAsync(IPAddress ipAddress)
    {
        _context.IPAddresses.Add(ipAddress);
        await _context.SaveChangesAsync();
    }

    //Buscar qual o country id no BD
    public async Task<Country?> GetCountryByCodeAsync(string twoLetterCode)
    {
        return await _context.Countries
            .FirstOrDefaultAsync(c => c.TwoLetterCode == twoLetterCode);
    }
}
