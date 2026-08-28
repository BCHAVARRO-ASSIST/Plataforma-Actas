using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data;

public sealed class RolRepository : IRolRepository
{
    private readonly PlataformaActasDbContext _dbContext;

    public RolRepository(PlataformaActasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .Include(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.Nombre.ToLower() == nombre.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Rol> CreateAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Add(rol);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return rol;
    }

    public async Task<Rol> UpdateAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Update(rol);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return rol;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rol == null)
        {
            return false;
        }

        _dbContext.Roles.Remove(rol);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HasAssignedUsersAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UsuarioRoles
            .AsNoTracking()
            .AnyAsync(ur => ur.RolId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Permiso>> GetAllPermisosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permisos
            .AsNoTracking()
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        var rol = await _dbContext.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);

        if (rol == null)
        {
            return false;
        }

        _dbContext.RolPermisos.RemoveRange(rol.RolPermisos);

        var validPermisos = await _dbContext.Permisos
            .Where(p => permisoIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var permiso in validPermisos)
        {
            _dbContext.RolPermisos.Add(new RolPermiso
            {
                RolId = rolId,
                PermisoId = permiso.Id
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
