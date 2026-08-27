using Npgsql;

namespace PlataformaActas.Api.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(NpgsqlDataSource dataSource)
    {
        try
        {
            await using var cmd = dataSource.CreateCommand();

            // 1. Asegurar permisos
            cmd.CommandText = """
                INSERT INTO permisos ("Nombre", "Modulo", "Descripcion", "Activo")
                SELECT 'DASHBOARD_CONSULTAR', 'Dashboard', 'Permite ver estadísticas y KPIs del sistema', true
                WHERE NOT EXISTS (SELECT 1 FROM permisos WHERE "Nombre" = 'DASHBOARD_CONSULTAR');

                INSERT INTO permisos ("Nombre", "Modulo", "Descripcion", "Activo")
                SELECT 'REPORTES_CONSULTAR', 'Reportes', 'Permite consultar y filtrar reportes consolidados', true
                WHERE NOT EXISTS (SELECT 1 FROM permisos WHERE "Nombre" = 'REPORTES_CONSULTAR');

                INSERT INTO permisos ("Nombre", "Modulo", "Descripcion", "Activo")
                SELECT 'REPORTES_EXPORTAR', 'Reportes', 'Permite exportar reportes a Excel y PDF', true
                WHERE NOT EXISTS (SELECT 1 FROM permisos WHERE "Nombre" = 'REPORTES_EXPORTAR');
                """;
            await cmd.ExecuteNonQueryAsync();

            // 2. Asociar permisos a roles existentes
            cmd.CommandText = """
                INSERT INTO rol_permisos ("RolId", "PermisoId")
                SELECT r."Id", p."Id"
                FROM roles r
                CROSS JOIN permisos p
                WHERE r."Nombre" IN ('Administrador', 'Auditor')
                  AND p."Nombre" IN ('DASHBOARD_CONSULTAR', 'REPORTES_CONSULTAR', 'REPORTES_EXPORTAR')
                  AND NOT EXISTS (
                      SELECT 1 FROM rol_permisos rp 
                      WHERE rp."RolId" = r."Id" AND rp."PermisoId" = p."Id"
                  );
                """;
            await cmd.ExecuteNonQueryAsync();

            // 3. Asegurar usuario admin con hash válido para "Admin123*"
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*");

            cmd.CommandText = """
                UPDATE usuarios 
                SET "PasswordHash" = @hash, "Activo" = TRUE
                WHERE "UsuarioRed" = 'admin';
                """;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@hash", passwordHash);
            var updated = await cmd.ExecuteNonQueryAsync();

            if (updated == 0)
            {
                cmd.CommandText = """
                    INSERT INTO usuarios ("Nombre", "Apellido", "UsuarioRed", "Correo", "Proceso", "Activo", "PasswordHash", "FechaCreacion")
                    VALUES ('Administrador', 'Sistema', 'admin', 'admin@plataformaactas.local', 'Tecnología', TRUE, @hash, CURRENT_TIMESTAMP);
                    """;
                await cmd.ExecuteNonQueryAsync();
            }

            // 4. Asegurar rol Administrador a usuario admin
            cmd.CommandText = """
                INSERT INTO usuario_roles ("UsuarioId", "RolId")
                SELECT u."Id", r."Id"
                FROM usuarios u, roles r
                WHERE u."UsuarioRed" = 'admin' AND r."Nombre" = 'Administrador'
                  AND NOT EXISTS (
                      SELECT 1 FROM usuario_roles ur
                      WHERE ur."UsuarioId" = u."Id" AND ur."RolId" = r."Id"
                  );
                """;
            cmd.Parameters.Clear();
            await cmd.ExecuteNonQueryAsync();

            Console.WriteLine("[DatabaseSeeder] Permisos y usuario admin sincronizados correctamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseSeeder Error] {ex.Message}");
        }
    }
}
