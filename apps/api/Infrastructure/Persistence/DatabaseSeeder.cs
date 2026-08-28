using Microsoft.EntityFrameworkCore;

namespace PlataformaActas.Api.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PlataformaActasDbContext context)
    {
        try
        {
            // 1. Asegurar Roles
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO roles ("Nombre", "Descripcion", "Activo", "FechaCreacion")
                VALUES 
                    ('Administrador', 'Acceso total y administración del sistema', true, CURRENT_TIMESTAMP),
                    ('Gestor', 'Gestión operativa de actas y equipos', true, CURRENT_TIMESTAMP),
                    ('Auditor', 'Consulta y visualización de reportes y auditoría', true, CURRENT_TIMESTAMP),
                    ('Mensajero', 'Recepción, firma y entrega de actas en campo', true, CURRENT_TIMESTAMP)
                ON CONFLICT ("Nombre") DO NOTHING;
                """);

            // 2. Asegurar Permisos
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO permisos ("Nombre", "Modulo", "Descripcion", "Activo")
                VALUES
                    ('DASHBOARD_CONSULTAR', 'Dashboard', 'Permite ver estadísticas y KPIs del sistema', true),
                    ('USUARIOS_CONSULTAR', 'Usuarios', 'Permite listar y consultar usuarios', true),
                    ('USUARIOS_REGISTRAR', 'Usuarios', 'Permite crear nuevos usuarios', true),
                    ('USUARIOS_EDITAR', 'Usuarios', 'Permite modificar usuarios existentes', true),
                    ('USUARIOS_DESACTIVAR', 'Usuarios', 'Permite desactivar usuarios', true),
                    ('EQUIPOS_CONSULTAR', 'Equipos', 'Permite listar y buscar equipos', true),
                    ('EQUIPOS_REGISTRAR', 'Equipos', 'Permite registrar nuevos equipos', true),
                    ('EQUIPOS_EDITAR', 'Equipos', 'Permite editar equipos', true),
                    ('EQUIPOS_DESACTIVAR', 'Equipos', 'Permite cambiar estado de equipos', true),
                    ('ACTAS_CONSULTAR', 'Actas', 'Permite listar y consultar actas', true),
                    ('ACTAS_REGISTRAR', 'Actas', 'Permite generar nuevas actas', true),
                    ('ACTAS_EDITAR', 'Actas', 'Permite actualizar actas', true),
                    ('ACTAS_FIRMAR', 'Actas', 'Permite firmar actas digitalmente', true),
                    ('ACTAS_EXPORTAR', 'Actas', 'Permite descargar actas en PDF', true),
                    ('REPORTES_CONSULTAR', 'Reportes', 'Permite consultar reportes consolidados', true),
                    ('REPORTES_EXPORTAR', 'Reportes', 'Permite exportar reportes a Excel y PDF', true),
                    ('ROLES_CONSULTAR', 'Roles', 'Permite consultar roles y permisos', true),
                    ('ROLES_CREAR', 'Roles', 'Permite crear roles', true),
                    ('ROLES_EDITAR', 'Roles', 'Permite modificar roles y asignar permisos', true),
                    ('ROLES_ELIMINAR', 'Roles', 'Permite eliminar roles', true),
                    ('MENSAJERO_ACTAS_CONSULTAR', 'Mensajero', 'Permite consultar actas asignadas a mensajería', true),
                    ('MENSAJERO_ACTAS_FIRMAR', 'Mensajero', 'Permite registrar firmas en entregas', true),
                    ('MENSAJERO_ACTAS_ENTREGAR', 'Mensajero', 'Permite completar entrega de actas', true),
                    ('MENSAJERO_ACTAS_DEVOLVER', 'Mensajero', 'Permite registrar devolución de actas', true)
                ON CONFLICT DO NOTHING;
                """);

            // 3. Asociar todos los permisos al rol Administrador
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO rol_permisos ("RolId", "PermisoId")
                SELECT r."Id", p."Id"
                FROM roles r
                CROSS JOIN permisos p
                WHERE r."Nombre" = 'Administrador'
                  AND NOT EXISTS (
                      SELECT 1 FROM rol_permisos rp 
                      WHERE rp."RolId" = r."Id" AND rp."PermisoId" = p."Id"
                  );
                """);

            // 4. Asegurar usuario admin con hash válido para "Admin123*"
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*");

            var updated = await context.Database.ExecuteSqlRawAsync("""
                UPDATE usuarios 
                SET "PasswordHash" = {0}, "Activo" = TRUE
                WHERE "UsuarioRed" = 'admin';
                """, passwordHash);

            if (updated == 0)
            {
                await context.Database.ExecuteSqlRawAsync("""
                    INSERT INTO usuarios ("Nombre", "Apellido", "UsuarioRed", "Correo", "Proceso", "Activo", "PasswordHash", "FechaCreacion")
                    VALUES ('Administrador', 'Sistema', 'admin', 'admin@plataformaactas.local', 'Tecnología', TRUE, {0}, CURRENT_TIMESTAMP);
                    """, passwordHash);
            }

            // 5. Asegurar rol Administrador a usuario admin
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO usuario_roles ("UsuarioId", "RolId")
                SELECT u."Id", r."Id"
                FROM usuarios u, roles r
                WHERE u."UsuarioRed" = 'admin' AND r."Nombre" = 'Administrador'
                  AND NOT EXISTS (
                      SELECT 1 FROM usuario_roles ur
                      WHERE ur."UsuarioId" = u."Id" AND ur."RolId" = r."Id"
                  );
                """);

            Console.WriteLine("[DatabaseSeeder] Permisos, roles y usuario admin sincronizados con éxito.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseSeeder Error] {ex.Message}");
        }
    }
}
