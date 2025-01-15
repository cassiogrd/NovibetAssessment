using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Net;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet para as tabelas do banco
    public DbSet<Country> Countries { get; set; }
    public DbSet<IPAddress> IPAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de chave estrangeira
        modelBuilder.Entity<IPAddress>()
            .HasOne(ip => ip.Country)
            .WithMany()
            .HasForeignKey(ip => ip.CountryId);
    }
}
