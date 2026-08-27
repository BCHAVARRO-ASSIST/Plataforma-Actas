using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Equipment.Data.Entities;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;

namespace PlataformaActas.Api.Modules.Equipment.Data;

public sealed class EquipmentRepository : IEquipmentRepository
{
    private readonly PlataformaActasDbContext _context;

    public EquipmentRepository(PlataformaActasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Entities.Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.NombreEquipo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Entities.Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Entities.Equipment?> GetByInventoryKeyAsync(string inventoryKey, CancellationToken cancellationToken = default)
    {
        var trimmed = inventoryKey.Trim().ToLower();
        return await _context.Equipment
            .FirstOrDefaultAsync(e => e.InventoryKey.ToLower() == trimmed, cancellationToken);
    }

    public async Task<Entities.Equipment> AddAsync(Entities.Equipment equipment, CancellationToken cancellationToken = default)
    {
        await _context.Equipment.AddAsync(equipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return equipment;
    }

    public async Task UpdateAsync(Entities.Equipment equipment, CancellationToken cancellationToken = default)
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

    public async Task<bool> HasActiveActasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // When Actas entity is integrated, this uses the DbSet. For now or with EF Core:
        return await _context.Database.SqlQueryRaw<int>(
            @"SELECT 1 FROM actas_v2 WHERE equipment_id = {0} AND estado != 'Anulada' LIMIT 1", id)
            .AnyAsync(cancellationToken);
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
