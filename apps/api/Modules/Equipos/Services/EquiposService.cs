using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlataformaActas.Api.Modules.Equipos.Data;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipos.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Equipos.Models.Dto;
using PlataformaActas.Api.Modules.Equipos.Models.Mappers;

namespace PlataformaActas.Api.Modules.Equipos.Services;

public sealed class EquiposService(IEquiposRepository equiposRepository) : IEquiposService
{
    public async Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await equiposRepository.GetAllAsync(cancellationToken);
        return list.Select(e => e.ToResponse()).ToList();
    }

    public async Task<EquipmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eq = await equiposRepository.GetByIdAsync(id, cancellationToken);
        return eq?.ToResponse();
    }

    public async Task<EquipmentResponse?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default)
    {
        var eq = await equiposRepository.GetByInventoryKeyAsync(inventoryKey, cancellationToken);
        return eq?.ToResponse();
    }

    public async Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await equiposRepository.GetByInventoryKeyAsync(request.InventoryKey, cancellationToken);
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

        var created = await equiposRepository.AddAsync(eq, cancellationToken);
        return created.ToResponse();
    }

    public async Task<EquipmentResponse?> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var eq = await equiposRepository.GetByIdAsync(id, cancellationToken);
        if (eq is null)
        {
            return null;
        }

        var existingWithKey = await equiposRepository.GetByInventoryKeyAsync(request.InventoryKey, cancellationToken);
        if (existingWithKey != null && existingWithKey.Id != id)
        {
            throw new ArgumentException($"Ya existe otro equipo con la clave de inventario '{request.InventoryKey}'.");
        }

        if (!string.IsNullOrWhiteSpace(request.EstatusEquipo) &&
            (request.EstatusEquipo.Equals("Dado de Baja", StringComparison.OrdinalIgnoreCase) ||
             request.EstatusEquipo.Equals("De Baja", StringComparison.OrdinalIgnoreCase) ||
             request.EstatusEquipo.Equals("Inactivo", StringComparison.OrdinalIgnoreCase)))
        {
            var hasActiveDelivery = await equiposRepository.HasActiveDeliveryActasAsync(id, cancellationToken);
            if (hasActiveDelivery)
            {
                throw new InvalidOperationException("No se puede dar de baja o inactivar un equipo que cuenta con un acta de Entrega activa pendiente de devolución. El equipo debe devolverse formalmente mediante un Acta de Devolución antes de darse de baja.");
            }
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

        await equiposRepository.UpdateAsync(eq, cancellationToken);
        return eq.ToResponse();
    }

    public async Task<bool> SetStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        if (status.Equals("Dado de Baja", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("De Baja", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
        {
            var hasActiveDelivery = await equiposRepository.HasActiveDeliveryActasAsync(id, cancellationToken);
            if (hasActiveDelivery)
            {
                throw new InvalidOperationException("No se puede dar de baja o inactivar un equipo que cuenta con un acta de Entrega activa pendiente de devolución. El equipo debe devolverse formalmente mediante un Acta de Devolución antes de darse de baja.");
            }
        }

        return await equiposRepository.SetStatusAsync(id, status, cancellationToken);
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await equiposRepository.SetStatusAsync(id, "Activo", cancellationToken);
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hasActiveDelivery = await equiposRepository.HasActiveDeliveryActasAsync(id, cancellationToken);
        if (hasActiveDelivery)
        {
            throw new InvalidOperationException("No se puede inactivar un equipo que cuenta con un acta de Entrega activa pendiente de devolución. El equipo debe devolverse formalmente mediante un Acta de Devolución antes de darse de baja.");
        }

        return await equiposRepository.SetStatusAsync(id, "Inactivo", cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hasDelivery = await equiposRepository.HasActiveDeliveryActasAsync(id, cancellationToken);
        if (hasDelivery)
        {
            throw new InvalidOperationException("No se puede eliminar un equipo que tiene actas de Entrega activas pendientes de devolución. El equipo debe devolverse formalmente mediante un Acta de Devolución antes de darse de baja.");
        }

        var hasActas = await equiposRepository.HasActiveActasAsync(id, cancellationToken);
        if (hasActas)
        {
            throw new InvalidOperationException("No se puede eliminar físicamente un equipo que tiene actas históricas asociadas. Considere cambiar su estado a Inactivo o Dado de Baja.");
        }

        return await equiposRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default)
    {
        return await equiposRepository.GetMaintenanceListAsync(cancellationToken);
    }

    public async Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        return await equiposRepository.UpdateMaintenanceAsync(id, request, cancellationToken);
    }
}
