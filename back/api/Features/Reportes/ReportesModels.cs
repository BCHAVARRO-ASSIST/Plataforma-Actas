namespace PlataformaActas.Api.Features.Reportes;

// =========================================================
// FILTROS DE REPORTES
// =========================================================

public sealed record ReportFilterRequest(
    DateTimeOffset? FechaInicio,
    DateTimeOffset? FechaFin,
    string? Estado,
    string? TipoActa,
    Guid? EquipmentId,
    int? MensajeroId,
    int? UsuarioId,
    string? Empresa,
    string? Proveedor,
    string? Marca,
    string? Search
);

// =========================================================
// REPORTE DE EQUIPOS
// =========================================================

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

// =========================================================
// REPORTE DE ACTAS
// =========================================================

public sealed record ActasReportItem(
    Guid Id,
    string NumeroActa,
    string TipoActa,
    string Estado,
    Guid EquipmentId,
    string? EquipmentInventoryKey,
    string? EquipmentNombre,
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

// =========================================================
// REPORTE DE ENTREGAS
// =========================================================

public sealed record EntregasReportItem(
    Guid Id,
    string NumeroActa,
    string EquipmentInventoryKey,
    string EquipmentNombre,
    string? MensajeroNombre,
    string? DestinatarioNombre,
    string Estado,
    bool Firmado,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaFirma
);

// =========================================================
// REPORTE DE DEVOLUCIONES
// =========================================================

public sealed record DevolucionesReportItem(
    Guid Id,
    string NumeroActa,
    Guid? ActaOrigenId,
    string? ActaOrigenNumero,
    string EquipmentInventoryKey,
    string EquipmentNombre,
    string? ResponsableEntrega,
    string Estado,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaFirma
);
