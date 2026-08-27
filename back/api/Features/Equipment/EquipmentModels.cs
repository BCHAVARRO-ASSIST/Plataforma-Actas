namespace PlataformaActas.Api.Features.Equipment;

// =====================================================
// RESPUESTA DE EQUIPO
// =====================================================

public sealed record EquipmentResponse(
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
    string? EstatusEquipo
);

// =====================================================
// REGISTRAR EQUIPO
// =====================================================

public sealed record CreateEquipmentRequest(
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
    string? EstatusEquipo
);

// =====================================================
// EDITAR EQUIPO
// =====================================================

public sealed record UpdateEquipmentRequest(
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
    string? EstatusEquipo
);

// =====================================================
// MANTENIMIENTO DE EQUIPOS
// =====================================================

public sealed record UpdateMaintenanceRequest(
    string TipoMantenimiento,
    DateOnly? FechaMantenimiento,
    bool MantenimientoRealizado
);

public sealed record EquipmentMaintenanceItem(
    Guid Id,
    string InventoryKey,
    string NombreEquipo,
    string? Marca,
    string? AsignadoA,
    string? Departamento,
    string? EstatusEquipo,
    string? TipoMantenimiento,
    DateOnly? FechaMantenimiento,
    bool MantenimientoRealizado
);