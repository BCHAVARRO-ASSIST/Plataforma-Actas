using PlataformaActas.Api.Modules.Equipos.Data.Entities;
using PlataformaActas.Api.Modules.Equipos.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipos.Models.Mappers;

public static class EquipoMappingExtensions
{
    public static EquipmentResponse ToResponse(this Equipo equipment)
    {
        return new EquipmentResponse(
            equipment.Id,
            equipment.InventoryKey,
            equipment.NombreEquipo,
            equipment.Empresa,
            equipment.AsignadoA,
            equipment.Proveedor,
            equipment.Marca,
            equipment.Procesador,
            equipment.DiscoDuro,
            equipment.MemoriaRam,
            equipment.Departamento,
            equipment.EstatusEquipo
        );
    }

    public static EquipoResponse ToEquipoResponse(this Equipo equipment)
    {
        return new EquipoResponse(
            equipment.Id,
            equipment.InventoryKey,
            equipment.NombreEquipo,
            equipment.Empresa,
            equipment.AsignadoA,
            equipment.Proveedor,
            equipment.Marca,
            equipment.Procesador,
            equipment.DiscoDuro,
            equipment.MemoriaRam,
            equipment.Departamento,
            equipment.EstatusEquipo
        );
    }
}
