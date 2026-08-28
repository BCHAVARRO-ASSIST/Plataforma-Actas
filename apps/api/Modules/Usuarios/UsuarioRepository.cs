using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Usuarios.Data;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly PlataformaActasDbContext _dbContext;

    public UsuarioRepository(PlataformaActasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.Nombre)
            .ThenBy(u => u.Apellido)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Usuarios
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> GetByUsuarioRedAsync(string usuarioRed, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Usuarios
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioRed.ToLower() == usuarioRed.ToLower(), cancellationToken);
    }

    public async Task<Usuario?> GetByCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Usuarios
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower(), cancellationToken);
    }

    public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _dbContext.Usuarios.Update(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario == null)
        {
            return false;
        }

        usuario.Activo = activo;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario == null)
        {
            return false;
        }

        var rol = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);
        if (rol == null)
        {
            return false;
        }

        _dbContext.UsuarioRoles.RemoveRange(usuario.UsuarioRoles);

        _dbContext.UsuarioRoles.Add(new UsuarioRol
        {
            UsuarioId = usuarioId,
            RolId = rolId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
