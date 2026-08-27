using Npgsql;

namespace PlataformaActas.Api.Infrastructure.Persistence;

public static class DatabaseConnection
{
    public static NpgsqlDataSource CreateDataSource(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "La cadena de conexión 'Postgres' no está configurada.");

        return NpgsqlDataSource.Create(connectionString);
    }
}