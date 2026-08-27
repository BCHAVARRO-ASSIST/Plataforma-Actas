using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly PlataformaActasDbContext _context;

    public AuthService(PlataformaActasDbContext context)
    {
        _context = context;
    }

    public async Task<LoginData?> AuthenticateAsync(
        string usuario,
        string password,
        CancellationToken cancellationToken = default)
    {
        var trimmed = usuario.Trim().ToLower();

        var user = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(
                u => u.UsuarioRed.ToLower() == trimmed || u.Correo.ToLower() == trimmed,
                cancellationToken);

        if (user is null || !user.Activo)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null;
        }

        var rolActivo = user.UsuarioRoles
            .Select(ur => ur.Rol)
            .FirstOrDefault(r => r != null && r.Activo);

        var usuarioData = new UsuarioAuthResponse(
            user.Id,
            user.Nombre,
            user.Apellido,
            user.UsuarioRed,
            user.Correo,
            user.Proceso
        );

        var rolData = new RolAuthResponse(
            rolActivo?.Id ?? 0,
            rolActivo?.Nombre ?? "Sin rol",
            rolActivo?.Descripcion ?? string.Empty
        );

        var permisos = rolActivo?.RolPermisos
            .Where(rp => rp.Permiso != null && rp.Permiso.Activo)
            .Select(rp => rp.Permiso.Nombre)
            .Distinct()
            .OrderBy(p => p)
            .ToList() ?? new List<string>();

        return new LoginData(usuarioData, rolData, permisos);
    }
}
