using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipos.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Equipos.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipos.Data;

public sealed class EquiposRepository : IEquiposRepository
{
    private readonly PlataformaActasDbContext _context;

    public EquiposRepository(PlataformaActasDbContext context)
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
        var eq = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (eq is null)
        {
            return false;
        }

        eq.EstatusEquipo = status;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eq = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (eq is null)
        {
            return false;
        }

        _context.Equipment.Remove(eq);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HasActiveActasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Actas.AnyAsync(
            a => a.EquipmentId == id && a.Estado != "Anulada",
            cancellationToken);
    }

    public async Task<bool> HasActiveDeliveryActasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var equipo = await _context.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (equipo != null && string.Equals(equipo.EstatusEquipo, "Asignado", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var hasDelivery = await _context.Actas.AsNoTracking()
            .AnyAsync(a => a.EquipmentId == id && a.TipoActa == "Entrega" && a.Estado != "Anulada", cancellationToken);

        if (!hasDelivery)
        {
            return false;
        }

        var latestActa = await _context.Actas.AsNoTracking()
            .Where(a => a.EquipmentId == id && a.Estado != "Anulada")
            .OrderByDescending(a => a.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestActa != null && string.Equals(latestActa.TipoActa, "Entrega", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public async Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .AsNoTracking()
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
            .OrderBy(e => e.InventoryKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var eq = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (eq is null)
        {
            return false;
        }

        eq.TipoMantenimiento = request.TipoMantenimiento;
        eq.FechaMantenimiento = request.FechaMantenimiento;
        eq.MantenimientoRealizado = request.MantenimientoRealizado;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
