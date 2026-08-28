using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipment.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipment.Data;

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

    public async Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.Equipment
            .AsNoTracking()
            .Where(e => e.MantenimientoRealizado == true || e.FechaMantenimiento != null ||
                        (e.EstatusEquipo != null && e.EstatusEquipo.ToLower().Contains("mantenimiento")))
            .OrderByDescending(e => e.FechaMantenimiento)
            .ToListAsync(cancellationToken);

        return list.Select(e => new EquipmentMaintenanceItem(
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
        )).ToList();
    }

    public async Task<bool> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var eq = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (eq is null)
        {
            return false;
        }

        eq.MantenimientoRealizado = request.MantenimientoRealizado;
        eq.FechaMantenimiento = request.FechaMantenimiento ?? DateOnly.FromDateTime(DateTime.UtcNow);
        eq.TipoMantenimiento = request.TipoMantenimiento;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
