using PlataformaActas.Api.Modules.Equipment.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipment.Services;

public interface IEquipmentService
{
    Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default);
}
