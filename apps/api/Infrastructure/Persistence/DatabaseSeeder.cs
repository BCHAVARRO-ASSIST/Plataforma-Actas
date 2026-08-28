using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Modules.Equipment.Data.Entities;
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

            // 5. Asegurar usuarios iniciales con contraseña "Admin123*"
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*");

            var usuariosSeed = new[]
            {
                new { Nombre = "Administrador", Apellido = "Sistema", UsuarioRed = "admin", Correo = "admin@plataformaactas.local", Proceso = "Tecnología", Rol = "Administrador" },
                new { Nombre = "Carlos", Apellido = "Gómez", UsuarioRed = "gestor", Correo = "gestor@plataformaactas.local", Proceso = "Operaciones TI", Rol = "Gestor" },
                new { Nombre = "Pedro", Apellido = "Ramírez", UsuarioRed = "mensajero", Correo = "mensajero@plataformaactas.local", Proceso = "Mensajería y Logística", Rol = "Mensajero" },
                new { Nombre = "Juan", Apellido = "Pérez", UsuarioRed = "juan.perez", Correo = "juan.perez@plataformaactas.local", Proceso = "Desarrollo", Rol = "Gestor" },
                new { Nombre = "María", Apellido = "Rodríguez", UsuarioRed = "maria.rodriguez", Correo = "maria.rodriguez@plataformaactas.local", Proceso = "Talento Humano", Rol = "Auditor" }
            };

            foreach (var u in usuariosSeed)
            {
                var existing = await context.Usuarios.FirstOrDefaultAsync(x => x.UsuarioRed == u.UsuarioRed);
                if (existing is null)
                {
                    var nuevo = new Usuario
                    {
                        Nombre = u.Nombre,
                        Apellido = u.Apellido,
                        UsuarioRed = u.UsuarioRed,
                        Correo = u.Correo,
                        Proceso = u.Proceso,
                        Activo = true,
                        PasswordHash = passwordHash,
                        FechaCreacion = DateTime.UtcNow
                    };
                    context.Usuarios.Add(nuevo);
                    await context.SaveChangesAsync();

                    var rol = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == u.Rol);
                    if (rol != null)
                    {
                        context.UsuarioRoles.Add(new Modules.Roles.Data.Entities.UsuarioRol
                        {
                            UsuarioId = nuevo.Id,
                            RolId = rol.Id
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 6. Asegurar Inventario de Equipos Tecnológicos inicial
            if (!await context.Equipment.AnyAsync())
            {
                var equipos = new List<Equipment>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-LEN-001",
                        NombreEquipo = "Lenovo ThinkPad T14 Gen 3",
                        Empresa = "Sede Principal",
                        AsignadoA = "Juan Pérez",
                        Proveedor = "Lenovo Colombia",
                        Marca = "Lenovo",
                        Procesador = "Intel Core i7-1260P",
                        DiscoDuro = "512 GB SSD NVMe",
                        MemoriaRam = "16 GB DDR4",
                        Departamento = "Desarrollo y TI",
                        EstatusEquipo = "Activo",
                        ActaEntregada = true,
                        MantenimientoRealizado = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-DEL-002",
                        NombreEquipo = "Dell Latitude 5430",
                        Empresa = "Sede Principal",
                        AsignadoA = "María Rodríguez",
                        Proveedor = "Dell Technologies",
                        Marca = "Dell",
                        Procesador = "Intel Core i5-1245U",
                        DiscoDuro = "256 GB SSD",
                        MemoriaRam = "16 GB DDR4",
                        Departamento = "Talento Humano",
                        EstatusEquipo = "Activo",
                        ActaEntregada = true,
                        MantenimientoRealizado = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-HP-003",
                        NombreEquipo = "HP Z2 G9 Workstation Tower",
                        Empresa = "Sede Norte",
                        AsignadoA = "Equipo Diseño Gráfico",
                        Proveedor = "HP Enterprise",
                        Marca = "HP",
                        Procesador = "Intel Core i9-13900K",
                        DiscoDuro = "1 TB SSD NVMe Gen4",
                        MemoriaRam = "32 GB DDR5",
                        Departamento = "Comunicaciones",
                        EstatusEquipo = "Activo",
                        ActaEntregada = true,
                        MantenimientoRealizado = true,
                        TipoMantenimiento = "Preventivo",
                        FechaMantenimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-APL-004",
                        NombreEquipo = "MacBook Pro 14 M3 Pro",
                        Empresa = "Sede Principal",
                        AsignadoA = "Dirección de Tecnología",
                        Proveedor = "Apple Colombia",
                        Marca = "Apple",
                        Procesador = "Apple M3 Pro (11-Core)",
                        DiscoDuro = "512 GB SSD",
                        MemoriaRam = "18 GB Unified",
                        Departamento = "Dirección TI",
                        EstatusEquipo = "Activo",
                        ActaEntregada = true,
                        MantenimientoRealizado = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-LEN-005",
                        NombreEquipo = "Lenovo ThinkBook 15 G2",
                        Empresa = "Sede Principal",
                        AsignadoA = "En Stock / Sin Asignar",
                        Proveedor = "Lenovo Colombia",
                        Marca = "Lenovo",
                        Procesador = "Intel Core i5-1135G7",
                        DiscoDuro = "256 GB SSD",
                        MemoriaRam = "8 GB DDR4",
                        Departamento = "Bodega TI",
                        EstatusEquipo = "Inactivo",
                        ActaEntregada = false,
                        MantenimientoRealizado = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-DEL-006",
                        NombreEquipo = "Monitor Dell UltraSharp U2723QE 27\" 4K",
                        Empresa = "Sede Principal",
                        AsignadoA = "Juan Pérez",
                        Proveedor = "Dell Technologies",
                        Marca = "Dell",
                        Procesador = "N/A",
                        DiscoDuro = "N/A",
                        MemoriaRam = "N/A",
                        Departamento = "Desarrollo y TI",
                        EstatusEquipo = "Activo",
                        ActaEntregada = true,
                        MantenimientoRealizado = false
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-HP-007",
                        NombreEquipo = "HP EliteBook 840 G8",
                        Empresa = "Sede Principal",
                        AsignadoA = "Mesa de Ayuda",
                        Proveedor = "HP Inc",
                        Marca = "HP",
                        Procesador = "Intel Core i7-1165G7",
                        DiscoDuro = "512 GB SSD",
                        MemoriaRam = "16 GB DDR4",
                        Departamento = "Soporte Técnico",
                        EstatusEquipo = "Mantenimiento",
                        ActaEntregada = false,
                        MantenimientoRealizado = true,
                        TipoMantenimiento = "Correctivo",
                        FechaMantenimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5))
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        InventoryKey = "EQ-SAM-008",
                        NombreEquipo = "Samsung Galaxy S23 5G Enterprise",
                        Empresa = "Sede Principal",
                        AsignadoA = "Pedro Ramírez",
                        Proveedor = "Samsung Electronics",
                        Marca = "Samsung",
                        Procesador = "Snapdragon 8 Gen 2",
                        DiscoDuro = "256 GB UFS",
                        MemoriaRam = "8 GB LPDDR5X",
                        Departamento = "Mensajería y Logística",
                        EstatusEquipo = "Activo",
                        ActaEntregada = true,
                        MantenimientoRealizado = false
                    }
                };

                context.Equipment.AddRange(equipos);
                await context.SaveChangesAsync();
            }

            Console.WriteLine("[DatabaseSeeder] Permisos, roles, usuarios y catálogo de equipos sincronizados con éxito.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseSeeder Error] {ex.Message}");
        }
    }
}
