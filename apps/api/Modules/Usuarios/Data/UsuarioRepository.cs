using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Usuarios.Data;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly PlataformaActasDbContext _context;

    public UsuarioRepository(PlataformaActasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.Nombre)
            .ThenBy(u => u.Apellido)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> GetByUsuarioRedAsync(string usuarioRed, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioRed.ToLower() == usuarioRed.ToLower(), cancellationToken);
    }

    public async Task<Usuario?> GetByCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower(), cancellationToken);
    }

    public async Task<Usuario?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var trimmed = identifier.Trim().ToLower();
        return await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.UsuarioRed.ToLower() == trimmed || u.Correo.ToLower() == trimmed, cancellationToken);
    }

    public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null)
        {
            return false;
        }

        usuario.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        if (usuario is null)
        {
            return false;
        }

        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == rolId && r.Activo, cancellationToken);
        if (rol is null)
        {
            throw new InvalidOperationException("El rol seleccionado no existe o está inactivo.");
        }

        var existingUserRoles = await _context.UsuarioRoles
            .Where(ur => ur.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

        _context.UsuarioRoles.RemoveRange(existingUserRoles);

        await _context.UsuarioRoles.AddAsync(new UsuarioRol
        {
            UsuarioId = usuarioId,
            RolId = rolId,
            FechaAsignacion = DateTime.UtcNow
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
