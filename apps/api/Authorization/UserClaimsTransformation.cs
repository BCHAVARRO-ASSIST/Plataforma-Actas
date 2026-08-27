using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;

namespace PlataformaActas.Api.Authorization;

public sealed class UserClaimsTransformation : IClaimsTransformation
{
    private readonly PlataformaActasDbContext _context;

    public UserClaimsTransformation(PlataformaActasDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return principal;
        }

        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return principal;
        }

        // Avoid duplicate resolution within the same request scope
        if (principal.HasClaim(c => c.Type == "permiso"))
        {
            return principal;
        }

        var user = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Id == userId && u.Activo);

        if (user is null)
        {
            return principal;
        }

        if (principal.Identity is ClaimsIdentity identity)
        {
            if (!principal.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UsuarioRed));
            }

            var rolActivo = user.UsuarioRoles
                .Select(ur => ur.Rol)
                .FirstOrDefault(r => r != null && r.Activo);

            if (rolActivo != null)
            {
                if (!principal.HasClaim(c => c.Type == ClaimTypes.Role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, rolActivo.Nombre));
                }

                foreach (var rp in rolActivo.RolPermisos.Where(rp => rp.Permiso != null && rp.Permiso.Activo))
                {
                    identity.AddClaim(new Claim("permiso", rp.Permiso.Nombre));
                }
            }
        }

        return principal;
    }
}
