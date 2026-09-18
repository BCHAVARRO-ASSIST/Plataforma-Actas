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
        var query = _context.Equipos.AsNoTracking().AsQueryable();

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

        return await query
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
    }

    public async Task<IReadOnlyList<ActasReportItem>> GetActasReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actas
            .AsNoTracking()
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
                (a.Equipo != null && a.Equipo.NombreEquipo.ToLower().Contains(s)) ||
                (a.DestinatarioUsuario != null && (a.DestinatarioUsuario.Nombre.ToLower().Contains(s) || a.DestinatarioUsuario.Apellido.ToLower().Contains(s))));
        }

        return await query
            .OrderByDescending(a => a.FechaCreacion)
            .Select(a => new ActasReportItem(
                a.Id,
                a.NumeroActa,
                a.TipoActa,
                a.Estado,
                a.EquipmentId,
                a.Equipo != null ? a.Equipo.InventoryKey : null,
                a.Equipo != null ? a.Equipo.NombreEquipo : null,
                a.UsuarioId,
                a.Usuario != null ? (a.Usuario.Nombre + " " + a.Usuario.Apellido).Trim() : null,
                a.DestinatarioUsuarioId,
                a.DestinatarioUsuario != null ? (a.DestinatarioUsuario.Nombre + " " + a.DestinatarioUsuario.Apellido).Trim() : null,
                a.MensajeroUsuarioId,
                a.MensajeroUsuario != null ? (a.MensajeroUsuario.Nombre + " " + a.MensajeroUsuario.Apellido).Trim() : null,
                a.ActaEntregada,
                a.FechaCreacion,
                a.FechaFirma,
                a.FechaFinalizacion,
                a.FechaAnulacion,
                a.ActaOrigenId
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EntregasReportItem>> GetEntregasReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actas
            .AsNoTracking()
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

        return await query
            .OrderByDescending(a => a.FechaCreacion)
            .Select(a => new EntregasReportItem(
                a.Id,
                a.NumeroActa,
                a.Equipo != null ? a.Equipo.InventoryKey : "-",
                a.Equipo != null ? a.Equipo.NombreEquipo : "-",
                a.MensajeroUsuario != null ? (a.MensajeroUsuario.Nombre + " " + a.MensajeroUsuario.Apellido).Trim() : null,
                a.DestinatarioUsuario != null ? (a.DestinatarioUsuario.Nombre + " " + a.DestinatarioUsuario.Apellido).Trim() : null,
                a.Estado,
                a.FechaFirma.HasValue,
                a.FechaCreacion,
                a.FechaFirma
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DevolucionesReportItem>> GetDevolucionesReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actas
            .AsNoTracking()
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

        return await query
            .OrderByDescending(a => a.FechaCreacion)
            .Select(a => new DevolucionesReportItem(
                a.Id,
                a.NumeroActa,
                a.ActaOrigenId,
                a.ActaOrigen != null ? a.ActaOrigen.NumeroActa : null,
                a.Equipo != null ? a.Equipo.InventoryKey : "-",
                a.Equipo != null ? a.Equipo.NombreEquipo : "-",
                a.DestinatarioUsuario != null ? (a.DestinatarioUsuario.Nombre + " " + a.DestinatarioUsuario.Apellido).Trim() : null,
                a.Estado,
                a.FechaCreacion,
                a.FechaFirma
            ))
            .ToListAsync(cancellationToken);
    }
}
