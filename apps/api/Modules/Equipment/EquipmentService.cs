using PlataformaActas.Api.Modules.Equipment.Data;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipment.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;
using PlataformaActas.Api.Modules.Equipment.Models.Mappers;

namespace PlataformaActas.Api.Modules.Equipment.Services;

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

    public async Task<EquipmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eq = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        return eq?.ToResponse();
    }

    public async Task<EquipmentResponse?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default)
    {
        var eq = await _equipmentRepository.GetByInventoryKeyAsync(inventoryKey, cancellationToken);
        return eq?.ToResponse();
    }

    public async Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _equipmentRepository.GetByInventoryKeyAsync(request.InventoryKey, cancellationToken);
        if (existing != null)
        {
            throw new ArgumentException($"Ya existe un equipo con la clave de inventario '{request.InventoryKey}'.");
        }

        var eq = new EquipmentEntity
        {
            Id = Guid.NewGuid(),
            InventoryKey = request.InventoryKey.Trim().ToUpper(),
            NombreEquipo = request.NombreEquipo.Trim(),
            Empresa = request.Empresa,
            AsignadoA = request.AsignadoA,
            Proveedor = request.Proveedor,
            Marca = request.Marca,
            Procesador = request.Procesador,
            DiscoDuro = request.DiscoDuro,
            MemoriaRam = request.MemoriaRam,
            Departamento = request.Departamento,
            EstatusEquipo = request.EstatusEquipo ?? "Activo"
        };

        var created = await _equipmentRepository.AddAsync(eq, cancellationToken);
        return created.ToResponse();
    }

    public async Task<EquipmentResponse?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var eq = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (eq is null)
        {
            return null;
        }

        eq.InventoryKey = request.InventoryKey.Trim().ToUpper();
        eq.NombreEquipo = request.NombreEquipo.Trim();
        eq.Empresa = request.Empresa;
        eq.AsignadoA = request.AsignadoA;
        eq.Proveedor = request.Proveedor;
        eq.Marca = request.Marca;
        eq.Procesador = request.Procesador;
        eq.DiscoDuro = request.DiscoDuro;
        eq.MemoriaRam = request.MemoriaRam;
        eq.Departamento = request.Departamento;
        eq.EstatusEquipo = request.EstatusEquipo ?? eq.EstatusEquipo;

        await _equipmentRepository.UpdateAsync(eq, cancellationToken);
        return eq.ToResponse();
    }

    public async Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        return await _equipmentRepository.SetStatusAsync(id, status, cancellationToken);
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _equipmentRepository.SetStatusAsync(id, "Activo", cancellationToken);
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _equipmentRepository.SetStatusAsync(id, "Inactivo", cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hasActas = await _equipmentRepository.HasActiveActasAsync(id, cancellationToken);
        if (hasActas)
        {
            throw new InvalidOperationException("No se puede eliminar el equipo porque tiene actas activas asociadas. Cambie su estado a 'Baja' o 'Inactivo'.");
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
