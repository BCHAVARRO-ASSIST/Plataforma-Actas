using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

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
                ON CONFLICT ("Nombre") DO NOTHING;
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

            // 4. Asociar permisos a Mensajero
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO rol_permisos ("RolId", "PermisoId")
                SELECT r."Id", p."Id"
                FROM roles r
                CROSS JOIN permisos p
                WHERE r."Nombre" = 'Mensajero'
                  AND p."Modulo" = 'Mensajero'
                  AND NOT EXISTS (
                      SELECT 1 FROM rol_permisos rp 
                      WHERE rp."RolId" = r."Id" AND rp."PermisoId" = p."Id"
                  );
                """);

            // 4.1 Asociar permisos a Gestor
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO rol_permisos ("RolId", "PermisoId")
                SELECT r."Id", p."Id"
                FROM roles r
                CROSS JOIN permisos p
                WHERE r."Nombre" = 'Gestor'
                  AND p."Nombre" IN ('DASHBOARD_CONSULTAR', 'EQUIPOS_CONSULTAR', 'EQUIPOS_REGISTRAR', 'EQUIPOS_EDITAR', 'ACTAS_CONSULTAR', 'ACTAS_REGISTRAR', 'ACTAS_EDITAR', 'ACTAS_FIRMAR', 'ACTAS_EXPORTAR', 'REPORTES_CONSULTAR', 'REPORTES_EXPORTAR')
                  AND NOT EXISTS (
                      SELECT 1 FROM rol_permisos rp 
                      WHERE rp."RolId" = r."Id" AND rp."PermisoId" = p."Id"
                  );
                """);

            // 4.2 Asociar permisos a Auditor
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO rol_permisos ("RolId", "PermisoId")
                SELECT r."Id", p."Id"
                FROM roles r
                CROSS JOIN permisos p
                WHERE r."Nombre" = 'Auditor'
                  AND p."Nombre" IN ('DASHBOARD_CONSULTAR', 'EQUIPOS_CONSULTAR', 'ACTAS_CONSULTAR', 'REPORTES_CONSULTAR', 'REPORTES_EXPORTAR', 'USUARIOS_CONSULTAR', 'ROLES_CONSULTAR')
                  AND NOT EXISTS (
                      SELECT 1 FROM rol_permisos rp 
                      WHERE rp."RolId" = r."Id" AND rp."PermisoId" = p."Id"
                  );
                """);

            // 5. Asegurar usuario Administrador esencial
            var adminUser = await context.Usuarios.FirstOrDefaultAsync(x => x.UsuarioRed == "admin");
            if (adminUser is null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*");
                var nuevoAdmin = new Usuario
                {
                    Nombre = "Administrador",
                    Apellido = "Sistema",
                    UsuarioRed = "admin",
                    Correo = "admin@plataformaactas.local",
                    Proceso = "Tecnología",
                    Activo = true,
                    PasswordHash = passwordHash,
                    FechaCreacion = DateTime.UtcNow
                };
                context.Usuarios.Add(nuevoAdmin);
                await context.SaveChangesAsync();

                var rolAdmin = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador");
                if (rolAdmin != null)
                {
                    context.UsuarioRoles.Add(new Modules.Roles.Data.Entities.UsuarioRol
                    {
                        UsuarioId = nuevoAdmin.Id,
                        RolId = rolAdmin.Id
                    });
                    await context.SaveChangesAsync();
                }
            }

            Console.WriteLine("[DatabaseSeeder] Roles, permisos y cuenta de administrador asegurados con éxito.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseSeeder Error] {ex.Message}");
        }
    }
}
