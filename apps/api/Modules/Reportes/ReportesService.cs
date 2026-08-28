using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Reportes.Models.Dto;

namespace PlataformaActas.Api.Modules.Reportes.Services;

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
            var s = filter.Search.Trim().ToLower();
            query = query.Where(e =>
                e.InventoryKey.ToLower().Contains(s) ||
                e.NombreEquipo.ToLower().Contains(s) ||
                (e.AsignadoA != null && e.AsignadoA.ToLower().Contains(s)));
        }

        var list = await query.OrderBy(e => e.InventoryKey).ToListAsync(cancellationToken);

        return list.Select(e => new EquiposReportItem(
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
        )).ToList();
    }

    public async Task<IReadOnlyList<ActasReportItem>> GetActasReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actas
            .AsNoTracking()
            .Include(a => a.Equipment)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.MensajeroUsuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            var estado = filter.Estado.Trim().ToLower();
            query = query.Where(a => a.Estado.ToLower() == estado);
        }

        if (!string.IsNullOrWhiteSpace(filter.TipoActa))
        {
            var tipo = filter.TipoActa.Trim().ToLower();
            query = query.Where(a => a.TipoActa.ToLower() == tipo);
        }

        if (filter.EquipmentId.HasValue)
        {
            query = query.Where(a => a.EquipmentId == filter.EquipmentId.Value);
        }

        if (filter.MensajeroId.HasValue)
        {
            query = query.Where(a => a.MensajeroUsuarioId == filter.MensajeroId.Value);
        }

        if (filter.FechaInicio.HasValue)
        {
            query = query.Where(a => a.FechaCreacion >= filter.FechaInicio.Value);
        }

        if (filter.FechaFin.HasValue)
        {
            query = query.Where(a => a.FechaCreacion <= filter.FechaFin.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(a =>
                (a.NumeroActa != null && a.NumeroActa.ToLower().Contains(s)) ||
                (a.Equipment != null && a.Equipment.NombreEquipo.ToLower().Contains(s)) ||
                (a.DestinatarioUsuario != null && (a.DestinatarioUsuario.Nombre.ToLower().Contains(s) || a.DestinatarioUsuario.Apellido.ToLower().Contains(s))));
        }

        var list = await query.OrderByDescending(a => a.FechaCreacion).ToListAsync(cancellationToken);

        return list.Select(a => new ActasReportItem(
            a.Id,
            a.NumeroActa,
            a.TipoActa,
            a.Estado,
            a.EquipmentId,
            a.Equipment?.InventoryKey,
            a.Equipment?.NombreEquipo,
            a.UsuarioId,
            a.Usuario != null ? $"{a.Usuario.Nombre} {a.Usuario.Apellido}".Trim() : null,
            a.DestinatarioUsuarioId,
            a.DestinatarioUsuario != null ? $"{a.DestinatarioUsuario.Nombre} {a.DestinatarioUsuario.Apellido}".Trim() : null,
            a.MensajeroUsuarioId,
            a.MensajeroUsuario != null ? $"{a.MensajeroUsuario.Nombre} {a.MensajeroUsuario.Apellido}".Trim() : null,
            a.ActaEntregada,
            a.FechaCreacion,
            a.FechaFirma,
            a.FechaFinalizacion,
            a.FechaAnulacion,
            a.ActaOrigenId
        )).ToList();
    }

    public async Task<IReadOnlyList<EntregasReportItem>> GetEntregasReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actas
            .AsNoTracking()
            .Include(a => a.Equipment)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.MensajeroUsuario)
            .Where(a => a.TipoActa == "Entrega")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            var estado = filter.Estado.Trim().ToLower();
            query = query.Where(a => a.Estado.ToLower() == estado);
        }

        if (filter.FechaInicio.HasValue)
        {
            query = query.Where(a => a.FechaCreacion >= filter.FechaInicio.Value);
        }

        if (filter.FechaFin.HasValue)
        {
            query = query.Where(a => a.FechaCreacion <= filter.FechaFin.Value);
        }

        var list = await query.OrderByDescending(a => a.FechaCreacion).ToListAsync(cancellationToken);

        return list.Select(a => new EntregasReportItem(
            a.Id,
            a.NumeroActa,
            a.Equipment?.InventoryKey ?? "-",
            a.Equipment?.NombreEquipo ?? "-",
            a.MensajeroUsuario != null ? $"{a.MensajeroUsuario.Nombre} {a.MensajeroUsuario.Apellido}".Trim() : null,
            a.DestinatarioUsuario != null ? $"{a.DestinatarioUsuario.Nombre} {a.DestinatarioUsuario.Apellido}".Trim() : null,
            a.Estado,
            a.FechaFirma.HasValue,
            a.FechaCreacion,
            a.FechaFirma
        )).ToList();
    }

    public async Task<IReadOnlyList<DevolucionesReportItem>> GetDevolucionesReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actas
            .AsNoTracking()
            .Include(a => a.Equipment)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.ActaOrigen)
            .Where(a => a.TipoActa == "Devolución" || a.TipoActa == "Devolucion")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            var estado = filter.Estado.Trim().ToLower();
            query = query.Where(a => a.Estado.ToLower() == estado);
        }

        if (filter.FechaInicio.HasValue)
        {
            query = query.Where(a => a.FechaCreacion >= filter.FechaInicio.Value);
        }

        if (filter.FechaFin.HasValue)
        {
            query = query.Where(a => a.FechaCreacion <= filter.FechaFin.Value);
        }

        var list = await query.OrderByDescending(a => a.FechaCreacion).ToListAsync(cancellationToken);

        return list.Select(a => new DevolucionesReportItem(
            a.Id,
            a.NumeroActa,
            a.ActaOrigenId,
            a.ActaOrigen?.NumeroActa,
            a.Equipment?.InventoryKey ?? "-",
            a.Equipment?.NombreEquipo ?? "-",
            a.DestinatarioUsuario != null ? $"{a.DestinatarioUsuario.Nombre} {a.DestinatarioUsuario.Apellido}".Trim() : null,
            a.Estado,
            a.FechaCreacion,
            a.FechaFirma
        )).ToList();
    }
}
