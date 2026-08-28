using EquipmentEntity = PlataformaActas.Api.Modules.Equipment.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipment.Data;

public interface IEquipmentRepository
{
    Task<IReadOnlyList<EquipmentEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EquipmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EquipmentEntity?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default);
    Task<EquipmentEntity> AddAsync(EquipmentEntity equipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(EquipmentEntity equipment, CancellationToken cancellationToken = default);
    Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveActasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
