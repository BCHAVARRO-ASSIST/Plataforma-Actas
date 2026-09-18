using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Dashboard.Models.Dto;

namespace PlataformaActas.Api.Modules.Dashboard.Services;

public sealed class DashboardService(PlataformaActasDbContext context) : IDashboardService
{

    public async Task<DashboardKpiResponse> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        var eqCounts = await context.Equipos
            .GroupBy(e => e.EstatusEquipo ?? "Sin Estado")
            .Select(g => new { Estado = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalEquipos = eqCounts.Sum(c => c.Count);
        var equiposActivos = eqCounts.Where(c => c.Estado == "Activo").Sum(c => c.Count);
        var equiposInactivos = eqCounts.Where(c => c.Estado == "Inactivo").Sum(c => c.Count);
        var equiposAsignados = eqCounts.Where(c => c.Estado == "Asignado").Sum(c => c.Count);
        
        var equiposMantenimiento = await context.Equipos
            .CountAsync(e => (e.EstatusEquipo != null && e.EstatusEquipo.Contains("Mantenimiento")) ||
                             (e.MantenimientoRealizado == true && e.FechaMantenimiento == null), cancellationToken);

        var actCounts = await context.Actas
            .GroupBy(a => a.Estado)
            .Select(g => new { Estado = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalActas = actCounts.Sum(c => c.Count);
        var actasBorrador = actCounts.Where(c => c.Estado == "Borrador").Sum(c => c.Count);
        var actasFirmadas = actCounts.Where(c => c.Estado == "Firmada").Sum(c => c.Count);
        var actasFinalizadas = actCounts.Where(c => c.Estado == "Finalizada").Sum(c => c.Count);
        var actasAnuladas = actCounts.Where(c => c.Estado == "Anulada").Sum(c => c.Count);

        var pendingActas = await context.Actas
            .Where(a => !a.ActaEntregada && a.Estado != "Anulada" && 
                        (a.TipoActa == "Entrega" || a.TipoActa == "Devolución" || a.TipoActa == "Devolucion"))
            .GroupBy(a => a.TipoActa)
            .Select(g => new { Tipo = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var entregasPendientes = pendingActas.Where(p => p.Tipo == "Entrega").Sum(p => p.Count);
        var devolucionesPendientes = pendingActas.Where(p => p.Tipo == "Devolución" || p.Tipo == "Devolucion").Sum(p => p.Count);

        return new DashboardKpiResponse(
            totalEquipos,
            equiposActivos,
            equiposInactivos,
            equiposAsignados,
            equiposMantenimiento,
            totalActas,
            actasBorrador,
            actasFirmadas,
            actasFinalizadas,
            actasAnuladas,
            entregasPendientes,
            devolucionesPendientes
        );
    }

    public async Task<IReadOnlyList<StatusCountItem>> GetEquipmentStatusChartAsync(CancellationToken cancellationToken = default)
    {
        var rawCounts = await context.Equipos
            .GroupBy(e => string.IsNullOrWhiteSpace(e.EstatusEquipo) ? "Sin Estado" : e.EstatusEquipo)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        var total = rawCounts.Sum(x => x.Cantidad);

        return rawCounts.Select(x => new StatusCountItem(
            x.Estado,
            x.Cantidad,
            total > 0 ? Math.Round((double)x.Cantidad / total * 100, 1) : 0,
            GetColorForEquipmentStatus(x.Estado)
        )).ToList();
    }

    public async Task<IReadOnlyList<StatusCountItem>> GetActasStatusChartAsync(CancellationToken cancellationToken = default)
    {
        var rawCounts = await context.Actas
            .GroupBy(a => string.IsNullOrWhiteSpace(a.Estado) ? "Sin Estado" : a.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        var total = rawCounts.Sum(x => x.Cantidad);

        return rawCounts.Select(x => new StatusCountItem(
            x.Estado,
            x.Cantidad,
            total > 0 ? Math.Round((double)x.Cantidad / total * 100, 1) : 0,
            GetColorForActaStatus(x.Estado)
        )).ToList();
    }

    public async Task<IReadOnlyList<MessengerDeliveryItem>> GetDeliveriesByMessengerAsync(CancellationToken cancellationToken = default)
    {
        var raw = await context.Actas
            .AsNoTracking()
            .Where(a => a.MensajeroUsuarioId != null && a.MensajeroUsuario != null)
            .GroupBy(a => new
            {
                Id = a.MensajeroUsuarioId!.Value,
                Nombre = a.MensajeroUsuario!.Nombre,
                Apellido = a.MensajeroUsuario!.Apellido
            })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Nombre,
                g.Key.Apellido,
                TotalAsignadas = g.Count(),
                TotalEntregadas = g.Count(a => a.ActaEntregada)
            })
            .OrderByDescending(r => r.TotalAsignadas)
            .ToListAsync(cancellationToken);

        return raw.Select(r => new MessengerDeliveryItem(
            r.Id,
            $"{r.Nombre} {r.Apellido}".Trim(),
            r.TotalAsignadas,
            r.TotalEntregadas,
            r.TotalAsignadas - r.TotalEntregadas
        )).ToList();
    }

    public async Task<IReadOnlyList<TimelinePointItem>> GetTimelineAsync(string? range, CancellationToken cancellationToken = default)
    {
        var days = range switch
        {
            "7d" => 7,
            "90d" => 90,
            _ => 30
        };

        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        var actas = await context.Actas
            .AsNoTracking()
            .Where(a => a.FechaCreacion >= startDate)
            .Select(a => new
            {
                a.FechaCreacion,
                a.Estado,
                a.TipoActa
            })
            .ToListAsync(cancellationToken);

        var grouped = actas
            .GroupBy(a => a.FechaCreacion.ToString("yyyy-MM-dd"))
            .Select(g => new TimelinePointItem(
                g.Key,
                g.Count(),
                g.Count(a => a.Estado == "Firmada" || a.Estado == "Finalizada"),
                g.Count(a => a.TipoActa == "Entrega"),
                g.Count(a => a.TipoActa == "Devolución" || a.TipoActa == "Devolucion")
            ))
            .OrderBy(t => t.Fecha)
            .ToList();

        return grouped;
    }

    public async Task<MaintenanceStatsResponse> GetMaintenanceStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalMantenimientos = await context.Equipos
            .CountAsync(e => e.MantenimientoRealizado == true || e.FechaMantenimiento != null, cancellationToken);

        var maintenanceStats = await context.Equipos
            .Where(e => e.TipoMantenimiento != null)
            .GroupBy(e => e.TipoMantenimiento)
            .Select(g => new { Tipo = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var preventivos = maintenanceStats.Where(m => m.Tipo != null && m.Tipo.Contains("Preventivo")).Sum(m => m.Count);
        var correctivos = maintenanceStats.Where(m => m.Tipo != null && m.Tipo.Contains("Correctivo")).Sum(m => m.Count);

        var pendientes = await context.Equipos
            .CountAsync(e => e.EstatusEquipo != null && e.EstatusEquipo.Contains("Mantenimiento"), cancellationToken);

        var concluidos = totalMantenimientos - pendientes;
        if (concluidos < 0) concluidos = 0;

        var tipos = new List<StatusCountItem>
        {
            new("Preventivo", preventivos, totalMantenimientos > 0 ? Math.Round((double)preventivos / totalMantenimientos * 100, 1) : 0, "#10b981"),
            new("Correctivo", correctivos, totalMantenimientos > 0 ? Math.Round((double)correctivos / totalMantenimientos * 100, 1) : 0, "#f59e0b")
        };

        return new MaintenanceStatsResponse(
            totalMantenimientos,
            preventivos,
            correctivos,
            concluidos,
            pendientes,
            tipos
        );
    }

    public async Task<IReadOnlyList<RecentActivityItem>> GetRecentActivityAsync(int limit, CancellationToken cancellationToken = default)
    {
        var take = limit > 0 ? Math.Min(limit, 50) : 10;

        var actas = await context.Actas
            .AsNoTracking()
            .OrderByDescending(a => a.FechaCreacion)
            .Take(take)
            .Select(a => new
            {
                Id = a.Id.ToString(),
                a.TipoActa,
                a.NumeroActa,
                NombreEquipo = a.Equipo != null ? a.Equipo.NombreEquipo : "Equipo",
                UsuarioNombre = a.Usuario != null ? a.Usuario.Nombre : null,
                UsuarioApellido = a.Usuario != null ? a.Usuario.Apellido : null,
                a.FechaCreacion,
                a.Estado
            })
            .ToListAsync(cancellationToken);

        return actas.Select(a =>
        {
            var responsable = string.IsNullOrWhiteSpace(a.UsuarioNombre) && string.IsNullOrWhiteSpace(a.UsuarioApellido)
                ? "Sistema"
                : $"{a.UsuarioNombre} {a.UsuarioApellido}".Trim();

            var numActa = !string.IsNullOrWhiteSpace(a.NumeroActa)
                ? a.NumeroActa
                : (a.Id.Length >= 8 ? a.Id[..8] : a.Id);

            var badge = a.Estado.ToLower() switch
            {
                "firmada" or "finalizada" => "success",
                "borrador" => "warning",
                "anulada" => "danger",
                _ => "info"
            };

            return new RecentActivityItem(
                a.Id,
                a.TipoActa,
                $"Acta {numActa}",
                $"{a.TipoActa} - {a.NombreEquipo}",
                responsable,
                a.FechaCreacion,
                a.Estado,
                badge
            );
        }).ToList();
    }

    private static string GetColorForEquipmentStatus(string status) => status.ToLower() switch
    {
        "activo" => "#10b981",
        "asignado" => "#3b82f6",
        "inactivo" => "#6b7280",
        "mantenimiento" => "#f59e0b",
        "baja" => "#ef4444",
        _ => "#8b5cf6"
    };

    private static string GetColorForActaStatus(string status) => status.ToLower() switch
    {
        "finalizada" or "firmada" => "#10b981",
        "borrador" => "#f59e0b",
        "anulada" => "#ef4444",
        _ => "#3b82f6"
    };
}
