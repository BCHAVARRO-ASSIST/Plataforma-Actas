using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Auth.Models.Dto;
using PlataformaActas.Api.Modules.Auth.Services;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly IOptions<GoogleIntegrationOptions> _googleOptions;
    private readonly IGoogleDirectoryService _directoryService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PlataformaActasDbContext _dbContext;

    public GoogleAuthService(
        IOptions<GoogleIntegrationOptions> googleOptions,
        IGoogleDirectoryService directoryService,
        IJwtTokenService jwtTokenService,
        PlataformaActasDbContext dbContext)
    {
        _googleOptions = googleOptions;
        _directoryService = directoryService;
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
    }

    public async Task<IResult> AuthenticateWithGoogleAsync(string? idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return Results.BadRequest(new { message = "El token criptográfico de Google es obligatorio." });
        }

        var options = _googleOptions.Value;
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings();

        if (!string.IsNullOrWhiteSpace(options.ClientId))
        {
            validationSettings.Audience = new[] { options.ClientId };
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
        }
        catch (Exception)
        {
            return Results.Json(
                new { message = "Token de autenticación de Google inválido, adulterado o expirado." },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
        {
            return Results.Json(
                new { message = "No fue posible verificar la identidad del colaborador con Google." },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var verifiedEmail = payload.Email.Trim().ToLowerInvariant();
        var expectedDomain = string.IsNullOrWhiteSpace(options.Domain) ? "assist.com.co" : options.Domain.Trim().ToLowerInvariant();

        if (!verifiedEmail.EndsWith($"@{expectedDomain}", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = $"El usuario autenticado ({verifiedEmail}) no pertenece al dominio corporativo @{expectedDomain}." });
        }

        // Validar estado con el Directorio de Google Workspace
        var validation = await _directoryService.ValidateUserAsync(verifiedEmail, cancellationToken);
        if (validation.Exists && !validation.IsActive)
        {
            var userToDeactivate = await _dbContext.Usuarios
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == verifiedEmail, cancellationToken);
            if (userToDeactivate != null && userToDeactivate.Activo)
            {
                userToDeactivate.Activo = false;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return Results.Json(
                new { message = "El colaborador se encuentra inactivo o suspendido en Google Workspace." },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Buscar usuario en base de datos local
        var usuario = await _dbContext.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == verifiedEmail, cancellationToken);

        if (usuario == null)
        {
            // Auto-aprovisionar colaborador verificado por Google Workspace
            var defaultRole = await _dbContext.Roles
                .Include(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(r => r.Nombre == "Gestor" && r.Activo, cancellationToken)
                ?? await _dbContext.Roles
                    .Include(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
                    .FirstOrDefaultAsync(r => r.Activo, cancellationToken);

            var given = !string.IsNullOrWhiteSpace(payload.GivenName)
                ? payload.GivenName
                : (!string.IsNullOrWhiteSpace(validation.GivenName) ? validation.GivenName : verifiedEmail.Split('@')[0]);

            var family = !string.IsNullOrWhiteSpace(payload.FamilyName)
                ? payload.FamilyName
                : (!string.IsNullOrWhiteSpace(validation.FamilyName) ? validation.FamilyName : "Consultor");

            usuario = new Usuario
            {
                Nombre = given,
                Apellido = family,
                UsuarioRed = verifiedEmail.Split('@')[0],
                Correo = verifiedEmail,
                Proceso = "Consultoría",
                Activo = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                FechaCreacion = DateTime.UtcNow
            };

            if (defaultRole != null)
            {
                usuario.UsuarioRoles.Add(new UsuarioRol
                {
                    Usuario = usuario,
                    Rol = defaultRole,
                    FechaAsignacion = DateTime.UtcNow
                });
            }

            _dbContext.Usuarios.Add(usuario);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!usuario.Activo)
        {
            usuario.Activo = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var rolActivo = usuario.UsuarioRoles
            .Select(ur => ur.Rol)
            .FirstOrDefault(r => r != null && r.Activo);

        var permisos = rolActivo?.RolPermisos
            .Where(rp => rp.Permiso != null && rp.Permiso.Activo)
            .Select(rp => rp.Permiso!.Nombre)
            .Distinct()
            .OrderBy(p => p)
            .ToList() ?? new List<string>();

        var usuarioAuth = new UsuarioAuthResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.UsuarioRed,
            usuario.Correo,
            usuario.Proceso
        );

        var rolAuth = rolActivo != null
            ? new RolAuthResponse(rolActivo.Id, rolActivo.Nombre, rolActivo.Descripcion)
            : null;

        var token = _jwtTokenService.GenerateToken(usuarioAuth, rolAuth, permisos);

        return Results.Ok(new LoginResponse(
            Token: token,
            Usuario: usuarioAuth,
            Rol: rolAuth,
            Permisos: permisos
        ));
    }
}
