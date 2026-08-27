namespace PlataformaActas.Api.Modules.Reportes.Models.Dto;

public sealed record ReportFilterRequest(
    DateTimeOffset? FechaInicio = null,
    DateTimeOffset? FechaFin = null,
    string? Estado = null,
    string? TipoActa = null,
    Guid? EquipmentId = null,
    int? MensajeroId = null,
    string? Empresa = null,
    string? Marca = null,
    string? Proveedor = null,
    string? Search = null
);

public sealed record EquiposReportItem(
    Guid Id,
    string InventoryKey,
    string NombreEquipo,
    string? Empresa,
    string? AsignadoA,
    string? Proveedor,
    string? Marca,
    string? Procesador,
    string? DiscoDuro,
    string? MemoriaRam,
    string? Departamento,
    string? EstatusEquipo,
    bool ActaEntregada,
    string? TipoMantenimiento,
    DateOnly? FechaMantenimiento
);

public sealed record ActasReportItem(
    Guid Id,
    string? NumeroActa,
    string TipoActa,
    string Estado,
    Guid EquipmentId,
    string? InventoryKey,
    string? NombreEquipo,
    int UsuarioId,
    string? CreadorNombre,
    int? DestinatarioUsuarioId,
    string? DestinatarioNombre,
    int? MensajeroUsuarioId,
    string? MensajeroNombre,
    bool ActaEntregada,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaFirma,
    DateTimeOffset? FechaFinalizacion,
    DateTimeOffset? FechaAnulacion,
    Guid? ActaOrigenId
);

public sealed record EntregasReportItem(
    Guid Id,
    string? NumeroActa,
    string EquipKey,
    string EquipNombre,
    string? MensajeroNombre,
    string? DestinatarioNombre,
    string Estado,
    bool Firmado,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaFirma
);

public sealed record DevolucionesReportItem(
    Guid Id,
    string? NumeroActa,
    Guid? ActaOrigenId,
    string? ActaOrigenNumero,
    string EquipKey,
    string EquipNombre,
    string? ResponsableEntrega,
    string Estado,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaFirma
);
