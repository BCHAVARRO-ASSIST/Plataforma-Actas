using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data;

public sealed class RolRepository : IRolRepository
{
    private readonly PlataformaActasDbContext _context;

    public RolRepository(PlataformaActasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Rol?> GetByIdWithPermisosAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Rol?> GetByNameAsync(string nombre, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
    }

    public async Task<Rol> AddAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(rol, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return rol;
    }

    public async Task UpdateAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(rol);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _context.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rol is null)
        {
            return false;
        }

        _context.RolPermisos.RemoveRange(rol.RolPermisos);
        _context.Roles.Remove(rol);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HasAssignedUsersAsync(int rolId, CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioRoles
            .AnyAsync(ur => ur.RolId == rolId, cancellationToken);
    }

    public async Task<IReadOnlyList<Permiso>> GetAllPermisosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permisos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);
        if (rol is null)
        {
            return false;
        }

        // Delete existing rol_permisos
        var existingPermisos = await _context.RolPermisos
            .Where(rp => rp.RolId == rolId)
            .ToListAsync(cancellationToken);

        _context.RolPermisos.RemoveRange(existingPermisos);

        // Fetch active permisos that match the provided ids
        var validPermisos = await _context.Permisos
            .Where(p => permisoIds.Contains(p.Id) && p.Activo)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var newRolPermisos = validPermisos.Select(permisoId => new RolPermiso
        {
            RolId = rolId,
            PermisoId = permisoId
        });

        await _context.RolPermisos.AddRangeAsync(newRolPermisos, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
