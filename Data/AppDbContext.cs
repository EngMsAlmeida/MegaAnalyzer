using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MegaAnalyzer.Models;

namespace MegaAnalyzer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Sorteio> Sorteios { get; set; }
    public DbSet<NumeroSorteado> NumerosSorteados { get; set; }
    public DbSet<Jogo> Jogos { get; set; }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=megaanalyzer.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}