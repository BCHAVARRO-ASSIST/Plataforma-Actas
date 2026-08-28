using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Reportes.Models.Dto;

namespace PlataformaActas.Api.Modules.Reportes.Services
{
    public interface IReportesService
    {
        Task<IReadOnlyList<EquiposReportItem>> GetEquiposReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ActasReportItem>> GetActasReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<EntregasReportItem>> GetEntregasReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DevolucionesReportItem>> GetDevolucionesReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
    }

    public sealed class ReportesService : IReportesService
    {
        private readonly PlataformaActasDbContext _context;

        public ReportesService(PlataformaActasDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<EquiposReportItem>> GetEquiposReportAsync(
            ReportFilterRequest filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Equipment.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Estado))
            {
                var estado = filter.Estado.Trim().ToLower();
                query = query.Where(e => e.EstatusEquipo != null && e.EstatusEquipo.ToLower() == estado);
            }

            if (!string.IsNullOrWhiteSpace(filter.Empresa))
            {
                var empresa = filter.Empresa.Trim().ToLower();
                query = query.Where(e => e.Empresa != null && e.Empresa.ToLower().Contains(empresa));
            }

            if (!string.IsNullOrWhiteSpace(filter.Marca))
            {
                var marca = filter.Marca.Trim().ToLower();
                query = query.Where(e => e.Marca != null && e.Marca.ToLower().Contains(marca));
            }

            if (!string.IsNullOrWhiteSpace(filter.Proveedor))
            {
                var proveedor = filter.Proveedor.Trim().ToLower();
                query = query.Where(e => e.Proveedor != null && e.Proveedor.ToLower().Contains(proveedor));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(e =>
                    e.InventoryKey.ToLower().Contains(search) ||
                    e.NombreEquipo.ToLower().Contains(search) ||
                    (e.AsignadoA != null && e.AsignadoA.ToLower().Contains(search)));
            }

            var list = await query
                .OrderBy(e => e.InventoryKey)
                .Select(e => new EquiposReportItem(
                    e.Id,
                    e.InventoryKey,
                    e.NombreEquipo,
                    e.Empresa,
                    e.AsignadoA,
                    e.Proveedor,
                    e.Marca,
                    e.Procesador,
                    e.DiscoDuro,
                    e.MemoriaRam,
                    e.Departamento,
                    e.EstatusEquipo,
                    e.ActaEntregada ?? false,
                    e.TipoMantenimiento,
                    e.FechaMantenimiento
                ))
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task<IReadOnlyList<ActasReportItem>> GetActasReportAsync(
            ReportFilterRequest filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Actas.AsNoTracking()
                .Include(a => a.Equipment)
                .Include(a => a.Usuario)
                .Include(a => a.DestinatarioUsuario)
                .Include(a => a.MensajeroUsuario)
                .AsQueryable();

            if (filter.FechaInicio.HasValue)
            {
                query = query.Where(a => a.FechaCreacion >= filter.FechaInicio.Value);
            }

            if (filter.FechaFin.HasValue)
            {
                query = query.Where(a => a.FechaCreacion <= filter.FechaFin.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Estado))
            {
                var estado = filter.Estado.Trim().ToLower();
                query = query.Where(a => a.Estado.ToLower() == estado);
            }

            if (!string.IsNullOrWhiteSpace(filter.TipoActa))
            {
                var tipo = filter.TipoActa.Trim().ToLower();
                query = query.Where(a => a.TipoActa.ToLower() == tipo ||
                    (tipo == "devolucion" && a.TipoActa.ToLower() == "devolución") ||
                    (tipo == "devolución" && a.TipoActa.ToLower() == "devolucion"));
            }

            if (filter.EquipmentId.HasValue)
            {
                query = query.Where(a => a.EquipmentId == filter.EquipmentId.Value);
            }

            if (filter.MensajeroId.HasValue)
            {
                query = query.Where(a => a.MensajeroUsuarioId == filter.MensajeroId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(a =>
                    (a.NumeroActa != null && a.NumeroActa.ToLower().Contains(search)) ||
                    (a.Equipment != null && a.Equipment.InventoryKey.ToLower().Contains(search)) ||
                    (a.Equipment != null && a.Equipment.NombreEquipo.ToLower().Contains(search)));
            }

            var list = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Select(a => new ActasReportItem(
                    a.Id,
                    a.NumeroActa,
                    a.TipoActa,
                    a.Estado,
                    a.EquipmentId,
                    a.Equipment != null ? a.Equipment.InventoryKey : null,
                    a.Equipment != null ? a.Equipment.NombreEquipo : null,
                    a.UsuarioId,
                    a.Usuario != null ? a.Usuario.Nombre + " " + a.Usuario.Apellido : null,
                    a.DestinatarioUsuarioId,
                    a.DestinatarioUsuario != null ? a.DestinatarioUsuario.Nombre + " " + a.DestinatarioUsuario.Apellido : null,
                    a.MensajeroUsuarioId,
                    a.MensajeroUsuario != null ? a.MensajeroUsuario.Nombre + " " + a.MensajeroUsuario.Apellido : null,
                    a.ActaEntregada,
                    a.FechaCreacion,
                    a.FechaFirma,
                    a.FechaFinalizacion,
                    a.FechaAnulacion,
                    a.ActaOrigenId
                ))
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task<IReadOnlyList<EntregasReportItem>> GetEntregasReportAsync(
            ReportFilterRequest filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Actas.AsNoTracking()
                .Include(a => a.Equipment)
                .Include(a => a.DestinatarioUsuario)
                .Include(a => a.MensajeroUsuario)
                .Where(a => a.TipoActa.ToLower() == "entrega")
                .AsQueryable();

            if (filter.FechaInicio.HasValue)
            {
                query = query.Where(a => a.FechaCreacion >= filter.FechaInicio.Value);
            }

            if (filter.FechaFin.HasValue)
            {
                query = query.Where(a => a.FechaCreacion <= filter.FechaFin.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Estado))
            {
                var estado = filter.Estado.Trim().ToLower();
                query = query.Where(a => a.Estado.ToLower() == estado);
            }

            if (filter.MensajeroId.HasValue)
            {
                query = query.Where(a => a.MensajeroUsuarioId == filter.MensajeroId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(a =>
                    (a.NumeroActa != null && a.NumeroActa.ToLower().Contains(search)) ||
                    (a.Equipment != null && a.Equipment.InventoryKey.ToLower().Contains(search)));
            }

            var list = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Select(a => new EntregasReportItem(
                    a.Id,
                    a.NumeroActa,
                    a.Equipment != null ? a.Equipment.InventoryKey : "N/A",
                    a.Equipment != null ? a.Equipment.NombreEquipo : "N/A",
                    a.MensajeroUsuario != null ? a.MensajeroUsuario.Nombre + " " + a.MensajeroUsuario.Apellido : null,
                    a.DestinatarioUsuario != null ? a.DestinatarioUsuario.Nombre + " " + a.DestinatarioUsuario.Apellido : null,
                    a.Estado,
                    a.FechaFirma != null || a.Estado == "Firmada" || a.Estado == "Finalizada",
                    a.FechaCreacion,
                    a.FechaFirma
                ))
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task<IReadOnlyList<DevolucionesReportItem>> GetDevolucionesReportAsync(
            ReportFilterRequest filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Actas.AsNoTracking()
                .Include(a => a.ActaOrigen)
                .Include(a => a.Equipment)
                .Include(a => a.DestinatarioUsuario)
                .Where(a => a.TipoActa.ToLower() == "devolución" || a.TipoActa.ToLower() == "devolucion")
                .AsQueryable();

            if (filter.FechaInicio.HasValue)
            {
                query = query.Where(a => a.FechaCreacion >= filter.FechaInicio.Value);
            }

            if (filter.FechaFin.HasValue)
            {
                query = query.Where(a => a.FechaCreacion <= filter.FechaFin.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Estado))
            {
                var estado = filter.Estado.Trim().ToLower();
                query = query.Where(a => a.Estado.ToLower() == estado);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(a =>
                    (a.NumeroActa != null && a.NumeroActa.ToLower().Contains(search)) ||
                    (a.Equipment != null && a.Equipment.InventoryKey.ToLower().Contains(search)));
            }

            var list = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Select(a => new DevolucionesReportItem(
                    a.Id,
                    a.NumeroActa,
                    a.ActaOrigenId,
                    a.ActaOrigen != null ? a.ActaOrigen.NumeroActa : null,
                    a.Equipment != null ? a.Equipment.InventoryKey : "N/A",
                    a.Equipment != null ? a.Equipment.NombreEquipo : "N/A",
                    a.DestinatarioUsuario != null ? a.DestinatarioUsuario.Nombre + " " + a.DestinatarioUsuario.Apellido : null,
                    a.Estado,
                    a.FechaCreacion,
                    a.FechaFirma
                ))
                .ToListAsync(cancellationToken);

            return list;
        }
    }
}
