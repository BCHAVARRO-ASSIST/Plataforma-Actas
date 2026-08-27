using PlataformaActas.Api.Modules.Equipment.Data.Entities;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipment.Data;

public interface IEquipmentRepository
{
    Task<IReadOnlyList<Entities.Equipment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Entities.Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Equipment?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default);
    Task<Entities.Equipment> AddAsync(Entities.Equipment equipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entities.Equipment equipment, CancellationToken cancellationToken = default);
    Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<bool> HasActiveActasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
