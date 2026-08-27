using PlataformaActas.Api.Modules.Equipment.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipment.Models.Mappers;

public static class EquipmentMappingExtensions
{
    public static EquipmentResponse ToResponse(this Data.Entities.Equipment equipment)
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
}
