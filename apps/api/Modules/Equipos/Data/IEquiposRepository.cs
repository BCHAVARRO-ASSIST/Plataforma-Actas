using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlataformaActas.Api.Modules.Equipos.Data.Entities;
using PlataformaActas.Api.Modules.Equipos.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipos.Data;

public interface IEquiposRepository
{
    Task<IReadOnlyList<Equipo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Equipo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Equipo?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default);
    Task<Equipo> AddAsync(Equipo equipo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Equipo equipo, CancellationToken cancellationToken = default);
    Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveActasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveDeliveryActasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
