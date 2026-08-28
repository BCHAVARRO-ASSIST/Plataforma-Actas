using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Dashboard.Models.Dto;

namespace PlataformaActas.Api.Modules.Dashboard.Services
{
    public interface IDashboardService
    {
        Task<DashboardKpiResponse> GetKpisAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StatusCountItem>> GetEquipmentStatusChartAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StatusCountItem>> GetActasStatusChartAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MessengerDeliveryItem>> GetDeliveriesByMessengerAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TimelinePointItem>> GetTimelineAsync(string? range, CancellationToken cancellationToken = default);
        Task<MaintenanceStatsResponse> GetMaintenanceStatsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RecentActivityItem>> GetRecentActivityAsync(int limit, CancellationToken cancellationToken = default);
    }

    public sealed class DashboardService : IDashboardService
    {
        private readonly PlataformaActasDbContext _context;

        public DashboardService(PlataformaActasDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardKpiResponse> GetKpisAsync(CancellationToken cancellationToken = default)
        {
            var totalEquipos = await _context.Equipment.CountAsync(cancellationToken);
            var equiposActivos = await _context.Equipment
                .CountAsync(e => e.EstatusEquipo != null && e.EstatusEquipo.ToLower() == "activo", cancellationToken);
            var equiposInactivos = await _context.Equipment
                .CountAsync(e => e.EstatusEquipo != null && e.EstatusEquipo.ToLower() == "inactivo", cancellationToken);
            var equiposAsignados = await _context.Equipment
                .CountAsync(e => e.EstatusEquipo != null && e.EstatusEquipo.ToLower() == "asignado", cancellationToken);
            var equiposMantenimiento = await _context.Equipment
                .CountAsync(e => (e.EstatusEquipo != null && e.EstatusEquipo.ToLower().Contains("mantenimiento")) ||
                                 (e.MantenimientoRealizado == true && e.FechaMantenimiento == null), cancellationToken);

            var totalActas = await _context.Actas.CountAsync(cancellationToken);
            var actasBorrador = await _context.Actas
                .CountAsync(a => a.Estado.ToLower() == "borrador", cancellationToken);
            var actasFirmadas = await _context.Actas
                .CountAsync(a => a.Estado.ToLower() == "firmada", cancellationToken);
            var actasFinalizadas = await _context.Actas
                .CountAsync(a => a.Estado.ToLower() == "finalizada", cancellationToken);
            var actasAnuladas = await _context.Actas
                .CountAsync(a => a.Estado.ToLower() == "anulada", cancellationToken);

            var entregasPendientes = await _context.Actas
                .CountAsync(a => a.Estado.ToLower() == "borrador" && (a.TipoActa == null || a.TipoActa.ToLower() == "entrega"), cancellationToken);
            var devolucionesPendientes = await _context.Actas
                .CountAsync(a => a.Estado.ToLower() == "borrador" && (a.TipoActa.ToLower() == "devolución" || a.TipoActa.ToLower() == "devolucion"), cancellationToken);

            return new DashboardKpiResponse(
                TotalEquipos: totalEquipos,
                EquiposActivos: equiposActivos,
                EquiposInactivos: equiposInactivos,
                EquiposAsignados: equiposAsignados,
                EquiposMantenimiento: equiposMantenimiento,
                TotalActas: totalActas,
                ActasBorrador: actasBorrador,
                ActasFirmadas: actasFirmadas,
                ActasFinalizadas: actasFinalizadas,
                ActasAnuladas: actasAnuladas,
                EntregasPendientes: entregasPendientes,
                DevolucionesPendientes: devolucionesPendientes
            );
        }

        public async Task<IReadOnlyList<StatusCountItem>> GetEquipmentStatusChartAsync(CancellationToken cancellationToken = default)
        {
            var grouped = await _context.Equipment
                .GroupBy(e => string.IsNullOrWhiteSpace(e.EstatusEquipo) ? "Sin estado" : e.EstatusEquipo.Trim())
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync(cancellationToken);

            var total = grouped.Sum(x => x.Cantidad);

            return grouped.Select(x =>
            {
                var pct = total > 0 ? Math.Round((double)x.Cantidad / total * 100, 1) : 0;
                var color = x.Estado.ToLower() switch
                {
                    "activo" => "#10b981",
                    "inactivo" => "#ef4444",
                    "asignado" => "#3b82f6",
                    "mantenimiento" => "#f59e0b",
                    _ => "#6b7280"
                };
                return new StatusCountItem(x.Estado, x.Cantidad, pct, color);
            }).ToList();
        }

        public async Task<IReadOnlyList<StatusCountItem>> GetActasStatusChartAsync(CancellationToken cancellationToken = default)
        {
            var grouped = await _context.Actas
                .GroupBy(a => string.IsNullOrWhiteSpace(a.Estado) ? "Sin estado" : a.Estado.Trim())
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync(cancellationToken);

            var total = grouped.Sum(x => x.Cantidad);

            return grouped.Select(x =>
            {
                var pct = total > 0 ? Math.Round((double)x.Cantidad / total * 100, 1) : 0;
                var color = x.Estado.ToLower() switch
                {
                    "borrador" => "#f59e0b",
                    "firmada" => "#10b981",
                    "finalizada" => "#3b82f6",
                    "anulada" => "#ef4444",
                    _ => "#6b7280"
                };
                return new StatusCountItem(x.Estado, x.Cantidad, pct, color);
            }).ToList();
        }

        public async Task<IReadOnlyList<MessengerDeliveryItem>> GetDeliveriesByMessengerAsync(CancellationToken cancellationToken = default)
        {
            var grouped = await _context.Actas
                .Include(a => a.MensajeroUsuario)
                .GroupBy(a => new
                {
                    a.MensajeroUsuarioId,
                    Nombre = a.MensajeroUsuario != null
                        ? a.MensajeroUsuario.Nombre + " " + a.MensajeroUsuario.Apellido
                        : "Sin mensajero asignado"
                })
                .Select(g => new MessengerDeliveryItem(
                    g.Key.MensajeroUsuarioId,
                    g.Key.Nombre,
                    g.Count(),
                    g.Count(a => a.Estado == "Firmada" || a.Estado == "Finalizada" || a.ActaEntregada),
                    g.Count(a => a.Estado == "Borrador")
                ))
                .OrderByDescending(x => x.TotalAsignadas)
                .ToListAsync(cancellationToken);

            return grouped;
        }

        public async Task<IReadOnlyList<TimelinePointItem>> GetTimelineAsync(
            string? range,
            CancellationToken cancellationToken = default)
        {
            var dias = range?.ToLower() switch
            {
                "7d" => 7,
                "90d" => 90,
                "month" => DateTime.UtcNow.Day,
                _ => 30
            };

            var fechaCorte = DateTimeOffset.UtcNow.AddDays(-dias);

            var actas = await _context.Actas
                .Where(a => a.FechaCreacion >= fechaCorte)
                .Select(a => new { a.FechaCreacion, a.Estado, a.TipoActa })
                .ToListAsync(cancellationToken);

            var result = actas
                .GroupBy(a => a.FechaCreacion.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .Select(g => new TimelinePointItem(
                    Fecha: g.Key,
                    TotalCreadas: g.Count(),
                    TotalFirmadas: g.Count(a => a.Estado == "Firmada" || a.Estado == "Finalizada"),
                    TotalEntregas: g.Count(a => a.TipoActa != null && a.TipoActa.ToLower() == "entrega"),
                    TotalDevoluciones: g.Count(a => a.TipoActa != null && (a.TipoActa.ToLower() == "devolución" || a.TipoActa.ToLower() == "devolucion"))
                ))
                .ToList();

            return result;
        }

        public async Task<MaintenanceStatsResponse> GetMaintenanceStatsAsync(CancellationToken cancellationToken = default)
        {
            var maintenanceEquipos = await _context.Equipment
                .Where(e => e.TipoMantenimiento != null || e.MantenimientoRealizado != null || e.FechaMantenimiento != null)
                .ToListAsync(cancellationToken);

            var total = maintenanceEquipos.Count;
            var preventivos = maintenanceEquipos
                .Count(e => e.TipoMantenimiento != null && e.TipoMantenimiento.ToLower().Contains("preventivo"));
            var correctivos = maintenanceEquipos
                .Count(e => e.TipoMantenimiento != null && e.TipoMantenimiento.ToLower().Contains("correctivo"));
            var concluidos = maintenanceEquipos
                .Count(e => e.MantenimientoRealizado == true);
            var pendientes = maintenanceEquipos
                .Count(e => e.MantenimientoRealizado == false || (e.MantenimientoRealizado == null && e.TipoMantenimiento != null));

            var tiposGrouped = maintenanceEquipos
                .Where(e => e.TipoMantenimiento != null || e.MantenimientoRealizado == true)
                .GroupBy(e => string.IsNullOrWhiteSpace(e.TipoMantenimiento) ? "General / Sin especificar" : e.TipoMantenimiento.Trim())
                .OrderByDescending(g => g.Count())
                .Select(g =>
                {
                    var cant = g.Count();
                    var pct = total > 0 ? Math.Round((double)cant / total * 100, 1) : 0;
                    return new StatusCountItem(g.Key, cant, pct, "#6366f1");
                })
                .ToList();

            return new MaintenanceStatsResponse(
                Total: total,
                Preventivos: preventivos,
                Correctivos: correctivos,
                Concluidos: concluidos,
                Pendientes: pendientes,
                Tipos: tiposGrouped
            );
        }

        public async Task<IReadOnlyList<RecentActivityItem>> GetRecentActivityAsync(
            int limit,
            CancellationToken cancellationToken = default)
        {
            var clampedLimit = Math.Clamp(limit, 5, 50);

            var actas = await _context.Actas
                .Include(a => a.Equipment)
                .Include(a => a.Usuario)
                .OrderByDescending(a => a.FechaFirma ?? a.FechaAnulacion ?? a.FechaFinalizacion ?? a.FechaCreacion)
                .Take(clampedLimit)
                .ToListAsync(cancellationToken);

            return actas.Select(a =>
            {
                var tipoEvento = a.Estado switch
                {
                    "Borrador" => "Acta Creada",
                    "Firmada" => "Acta Firmada",
                    "Finalizada" => "Acta Finalizada",
                    "Anulada" => "Acta Anulada",
                    _ => "Actualización de Acta"
                };

                var fecha = a.FechaFirma ?? a.FechaAnulacion ?? a.FechaFinalizacion ?? a.FechaCreacion;
                var nombreEquipo = a.Equipment?.NombreEquipo ?? a.Equipment?.InventoryKey ?? "N/A";
                var responsable = a.Usuario != null ? $"{a.Usuario.Nombre} {a.Usuario.Apellido}" : "Sistema";

                return new RecentActivityItem(
                    Id: a.Id.ToString(),
                    TipoEvento: tipoEvento,
                    Titulo: $"Acta {a.NumeroActa} ({a.TipoActa})",
                    Descripcion: $"Equipo: {nombreEquipo}",
                    Responsable: responsable,
                    Fecha: fecha,
                    Estado: a.Estado,
                    BadgeTipo: a.TipoActa
                );
            }).ToList();
        }
    }
}
