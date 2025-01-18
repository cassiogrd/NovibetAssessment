using Microsoft.EntityFrameworkCore;
using Serilog;

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
        Log.Information("Buscando informações para o IP {Ip} no banco de dados.", ip);
        try
        {
            var result = await _context.IPAddresses
                .Include(i => i.Country)
                .FirstOrDefaultAsync(i => i.IP == ip);

            if (result != null)
                Log.Information("Dados do IP {Ip} encontrados no banco de dados.", ip);
            else
                Log.Warning("Nenhum dado encontrado no banco de dados para o IP {Ip}.", ip);

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao buscar informações do IP {Ip} no banco de dados.", ip);
            throw;
        }
    }

    // Salvar informações de IP no banco
    public async Task SaveIpInfoAsync(IPAddress ipAddress)
    {
        Log.Information("Salvando informações do IP {Ip} no banco de dados.", ipAddress.IP);
        try
        {
            _context.IPAddresses.Add(ipAddress);
            await _context.SaveChangesAsync();
            Log.Information("Informações do IP {Ip} salvas com sucesso.", ipAddress.IP);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao salvar informações do IP {Ip} no banco de dados.", ipAddress.IP);
            throw;
        }
    }

    // Buscar qual o country id no BD
    public async Task<Country?> GetCountryByCodeAsync(string twoLetterCode)
    {
        Log.Information("Buscando informações para o país com código {Code}.", twoLetterCode);
        try
        {
            var result = await _context.Countries
                .FirstOrDefaultAsync(c => c.TwoLetterCode == twoLetterCode);

            if (result != null)
                Log.Information("País encontrado para o código {Code}: {Country}.", twoLetterCode, result.Name);
            else
                Log.Warning("Nenhum país encontrado para o código {Code}.", twoLetterCode);

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao buscar país com código {Code} no banco de dados.", twoLetterCode);
            throw;
        }
    }

    // Buscar IP em Batchs
    public async Task<List<IPAddress>> GetIpsBatchAsync(int batchSize, int offset)
    {
        return await _context.IPAddresses
            .Include(i => i.Country)
            .OrderBy(i => i.Id)
            .Skip(offset)
            .Take(batchSize)
            .ToListAsync();
    }

    // Atualizar
    public async Task UpdateIpInfoAsync(IPAddress ipAddress)
    {
        _context.IPAddresses.Update(ipAddress);
        await _context.SaveChangesAsync();
    }
}
