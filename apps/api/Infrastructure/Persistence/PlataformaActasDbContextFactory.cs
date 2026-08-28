using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PlataformaActas.Api.Infrastructure.Persistence;

public class PlataformaActasDbContextFactory : IDesignTimeDbContextFactory<PlataformaActasDbContext>
{
    public PlataformaActasDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=plataforma_actas;Username=plataforma_user;Password=plataforma_dev_2026";

        var builder = new DbContextOptionsBuilder<PlataformaActasDbContext>();
        builder.UseNpgsql(connectionString);

        return new PlataformaActasDbContext(builder.Options);
    }
}
