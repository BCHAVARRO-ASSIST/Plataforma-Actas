using Npgsql;

namespace PlataformaActas.Api.Features.Auth;

public sealed class AuthService
{
    private readonly NpgsqlDataSource _dataSource;

    public AuthService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<LoginData?> AuthenticateAsync(
        string usuario,
        string password)
    {
        const string sql = """
            SELECT
                u."Id",
                u."Nombre",
                u."Apellido",
                u."UsuarioRed",
                u."Correo",
                u."Proceso",
                u."Activo",
                u."PasswordHash",

                r."Id" AS "RolId",
                r."Nombre" AS "RolNombre",
                r."Descripcion" AS "RolDescripcion"

            FROM usuarios u

            LEFT JOIN usuario_roles ur
                ON ur."UsuarioId" = u."Id"

            LEFT JOIN roles r
                ON r."Id" = ur."RolId"

            WHERE
                u."UsuarioRed" = @usuario
                OR LOWER(u."Correo") = LOWER(@usuario)

            LIMIT 1;
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(
                sql,
                connection);

        command.Parameters.AddWithValue(
            "usuario",
            usuario.Trim());

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        var activo =
            reader.GetBoolean(
                reader.GetOrdinal("Activo"));

        if (!activo)
        {
            return null;
        }

        var passwordHash =
            reader.IsDBNull(
                reader.GetOrdinal("PasswordHash"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("PasswordHash"));

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return null;
        }

        var passwordCorrecta =
            BCrypt.Net.BCrypt.Verify(
                password,
                passwordHash);

        if (!passwordCorrecta)
        {
            return null;
        }

        var usuarioId =
            reader.GetInt32(
                reader.GetOrdinal("Id"));

        var rolId =
            reader.IsDBNull(
                reader.GetOrdinal("RolId"))
                ? 0
                : reader.GetInt32(
                    reader.GetOrdinal("RolId"));

        var rolNombre =
            reader.IsDBNull(
                reader.GetOrdinal("RolNombre"))
                ? "Sin rol"
                : reader.GetString(
                    reader.GetOrdinal("RolNombre"));

        var rolDescripcion =
            reader.IsDBNull(
                reader.GetOrdinal("RolDescripcion"))
                ? ""
                : reader.GetString(
                    reader.GetOrdinal("RolDescripcion"));

        var usuarioData =
            new UsuarioAuthResponse(
                usuarioId,
                reader.GetString(
                    reader.GetOrdinal("Nombre")),
                reader.GetString(
                    reader.GetOrdinal("Apellido")),
                reader.GetString(
                    reader.GetOrdinal("UsuarioRed")),
                reader.GetString(
                    reader.GetOrdinal("Correo")),
                reader.GetString(
                    reader.GetOrdinal("Proceso"))
            );

        var rol =
            new RolAuthResponse(
                rolId,
                rolNombre,
                rolDescripcion
            );

        await reader.CloseAsync();

        var permisos =
            await ObtenerPermisosAsync(
                usuarioId,
                rolId);

        return new LoginData(
            usuarioData,
            rol,
            permisos);
    }

    private async Task<IReadOnlyList<string>>
        ObtenerPermisosAsync(
            int usuarioId,
            int rolId)
    {
        const string sql = """
            SELECT DISTINCT
                p."Nombre"

            FROM permisos p

            INNER JOIN rol_permisos rp
                ON rp."PermisoId" = p."Id"

            INNER JOIN usuario_roles ur
                ON ur."RolId" = rp."RolId"

            WHERE
                ur."UsuarioId" = @usuarioId
                AND p."Activo" = TRUE

            ORDER BY p."Nombre";
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(
                sql,
                connection);

        command.Parameters.AddWithValue(
            "usuarioId",
            usuarioId);

        var permisos =
            new List<string>();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            permisos.Add(
                reader.GetString(0));
        }

        return permisos;
    }
}

public sealed record LoginData(
    UsuarioAuthResponse Usuario,
    RolAuthResponse Rol,
    IReadOnlyList<string> Permisos
);
