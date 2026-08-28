using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipment.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;
using PlataformaActas.Api.Modules.Equipment.Models.Mappers;

namespace PlataformaActas.Api.Modules.Equipment.Data
{
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

    public sealed class EquipmentRepository : IEquipmentRepository
    {
        private readonly PlataformaActasDbContext _context;

        public EquipmentRepository(PlataformaActasDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<EquipmentEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Equipment
                .AsNoTracking()
                .OrderBy(e => e.InventoryKey)
                .ToListAsync(cancellationToken);
        }

        public async Task<EquipmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<EquipmentEntity?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default)
        {
            return await _context.Equipment
                .FirstOrDefaultAsync(e => e.InventoryKey.ToLower() == inventoryKey.ToLower(), cancellationToken);
        }

        public async Task<EquipmentEntity> AddAsync(EquipmentEntity equipment, CancellationToken cancellationToken = default)
        {
            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync(cancellationToken);
            return equipment;
        }

        public async Task UpdateAsync(EquipmentEntity equipment, CancellationToken cancellationToken = default)
        {
            _context.Equipment.Update(equipment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
        {
            var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (equipment is null)
            {
                return false;
            }

            equipment.EstatusEquipo = status;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (equipment is null)
            {
                return false;
            }

            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HasActiveActasAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Actas
                .AsNoTracking()
                .AnyAsync(a => a.EquipmentId == id && a.Estado.ToLower() != "anulada", cancellationToken);
        }

        public async Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Equipment
                .AsNoTracking()
                .OrderByDescending(e => e.FechaMantenimiento)
                .ThenBy(e => e.NombreEquipo)
                .Select(e => new EquipmentMaintenanceItem(
                    e.Id,
                    e.InventoryKey,
                    e.NombreEquipo,
                    e.Marca,
                    e.AsignadoA,
                    e.Departamento,
                    e.EstatusEquipo,
                    e.TipoMantenimiento,
                    e.FechaMantenimiento,
                    e.MantenimientoRealizado ?? false
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateMaintenanceAsync(
            Guid id,
            UpdateMaintenanceRequest request,
            CancellationToken cancellationToken = default)
        {
            var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (equipment is null)
            {
                return false;
            }

            equipment.TipoMantenimiento = request.TipoMantenimiento?.Trim();
            equipment.FechaMantenimiento = request.FechaMantenimiento;
            equipment.MantenimientoRealizado = request.MantenimientoRealizado;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

namespace PlataformaActas.Api.Modules.Equipment.Services
{
    using PlataformaActas.Api.Modules.Equipment.Data;

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

    public sealed class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentService(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }

        public async Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var list = await _equipmentRepository.GetAllAsync(cancellationToken);
            return list.Select(e => e.ToResponse()).ToList();
        }

        public async Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default)
        {
            var inventoryKey = request.InventoryKey.Trim();
            var nombreEquipo = request.NombreEquipo.Trim();

            if (string.IsNullOrWhiteSpace(inventoryKey))
            {
                throw new ArgumentException("El código de inventario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(nombreEquipo))
            {
                throw new ArgumentException("El nombre del equipo es obligatorio.");
            }

            var existing = await _equipmentRepository.GetByInventoryKeyAsync(inventoryKey, cancellationToken);
            if (existing is not null)
            {
                throw new InvalidOperationException("Ya existe un equipo con ese código de inventario.");
            }

            var equipment = new EquipmentEntity
            {
                Id = Guid.NewGuid(),
                InventoryKey = inventoryKey,
                NombreEquipo = nombreEquipo,
                Empresa = request.Empresa?.Trim(),
                AsignadoA = request.AsignadoA?.Trim(),
                Proveedor = request.Proveedor?.Trim(),
                Marca = request.Marca?.Trim(),
                Procesador = request.Procesador?.Trim(),
                DiscoDuro = request.DiscoDuro?.Trim(),
                MemoriaRam = request.MemoriaRam?.Trim(),
                Departamento = request.Departamento?.Trim(),
                EstatusEquipo = string.IsNullOrWhiteSpace(request.EstatusEquipo) ? "Activo" : request.EstatusEquipo.Trim()
            };

            var created = await _equipmentRepository.AddAsync(equipment, cancellationToken);
            return created.ToResponse();
        }

        public async Task<EquipmentResponse?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken = default)
        {
            var inventoryKey = request.InventoryKey.Trim();
            var nombreEquipo = request.NombreEquipo.Trim();

            if (string.IsNullOrWhiteSpace(inventoryKey))
            {
                throw new ArgumentException("El código de inventario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(nombreEquipo))
            {
                throw new ArgumentException("El nombre del equipo es obligatorio.");
            }

            var existingWithKey = await _equipmentRepository.GetByInventoryKeyAsync(inventoryKey, cancellationToken);
            if (existingWithKey is not null && existingWithKey.Id != id)
            {
                throw new InvalidOperationException("Ya existe otro equipo con ese código de inventario.");
            }

            var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
            if (equipment is null)
            {
                return null;
            }

            equipment.InventoryKey = inventoryKey;
            equipment.NombreEquipo = nombreEquipo;
            equipment.Empresa = request.Empresa?.Trim();
            equipment.AsignadoA = request.AsignadoA?.Trim();
            equipment.Proveedor = request.Proveedor?.Trim();
            equipment.Marca = request.Marca?.Trim();
            equipment.Procesador = request.Procesador?.Trim();
            equipment.DiscoDuro = request.DiscoDuro?.Trim();
            equipment.MemoriaRam = request.MemoriaRam?.Trim();
            equipment.Departamento = request.Departamento?.Trim();
            equipment.EstatusEquipo = string.IsNullOrWhiteSpace(request.EstatusEquipo) ? "Activo" : request.EstatusEquipo.Trim();

            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
            return equipment.ToResponse();
        }

        public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _equipmentRepository.SetStatusAsync(id, "Inactivo", cancellationToken);
        }

        public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _equipmentRepository.SetStatusAsync(id, "Activo", cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var hasActiveActas = await _equipmentRepository.HasActiveActasAsync(id, cancellationToken);
            if (hasActiveActas)
            {
                throw new InvalidOperationException("No es posible eliminar el equipo porque cuenta con actas activas registradas.");
            }

            return await _equipmentRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default)
        {
            return await _equipmentRepository.GetMaintenanceListAsync(cancellationToken);
        }

        public async Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default)
        {
            return await _equipmentRepository.UpdateMaintenanceAsync(id, request, cancellationToken);
        }
    }
}
