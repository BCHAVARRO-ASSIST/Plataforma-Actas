using System;

namespace PlataformaActas.Api.Modules.Equipos.Models.Dto;

public sealed record EquipoResponse(
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

public sealed record CreateEquipoRequest(
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

public sealed record UpdateEquipoRequest(
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

public sealed record EquipoMaintenanceItem(
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

public sealed record UpdateMaintenanceRequest(
    string? TipoMantenimiento,
    DateOnly? FechaMantenimiento,
    bool MantenimientoRealizado
);

// =====================================================
// ALIASES DE COMPATIBILIDAD RETROACTIVA
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
