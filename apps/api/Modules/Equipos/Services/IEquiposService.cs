using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlataformaActas.Api.Modules.Equipos.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipos.Services;

public interface IEquiposService
{
    Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default);
    Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default);
}
