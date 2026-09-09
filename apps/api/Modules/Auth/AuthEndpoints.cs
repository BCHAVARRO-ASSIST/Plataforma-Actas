using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Auth.Models.Dto;
using PlataformaActas.Api.Modules.Auth.Services;

namespace PlataformaActas.Api.Modules.Auth.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth");

            // 1. Login
            group.MapPost("/login", async (
                [FromBody] LoginRequest request,
                [FromServices] IAuthService authService,
                [FromServices] IJwtTokenService jwtTokenService,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(new { message = "El usuario y la contraseña son obligatorios." });
                }

                var login = await authService.AuthenticateAsync(request.Usuario, request.Password, cancellationToken);
                if (login is null)
                {
                    return Results.Unauthorized();
                }

                var token = jwtTokenService.GenerateToken(login.Usuario, login.Rol, login.Permisos);

                return Results.Ok(new LoginResponse(
                    Token: token,
                    Usuario: login.Usuario,
                    Rol: login.Rol,
                    Permisos: login.Permisos
                ));
            })
            .RequireRateLimiting("auth-limit");

            // 2. Usuario autenticado (me)
            group.MapGet("/me", async (
                HttpContext httpContext,
                [FromServices] PlataformaActasDbContext context,
                CancellationToken cancellationToken) =>
            {
                var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var user = await context.Usuarios
                    .AsNoTracking()
                    .Include(u => u.UsuarioRoles)
                        .ThenInclude(ur => ur.Rol)
                            .ThenInclude(r => r.RolPermisos)
                                .ThenInclude(rp => rp.Permiso)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user is null || !user.Activo)
                {
                    return Results.Unauthorized();
                }

                var rolActivo = user.UsuarioRoles
                    .Select(ur => ur.Rol)
                    .FirstOrDefault(r => r != null && r.Activo);

                var permisos = rolActivo?.RolPermisos
                    .Where(rp => rp.Permiso != null && rp.Permiso.Activo)
                    .Select(rp => rp.Permiso.Nombre)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList() ?? new List<string>();

                return Results.Ok(new
                {
                    usuarioId = user.Id.ToString(),
                    usuarioRed = user.UsuarioRed,
                    nombre = user.Nombre,
                    apellido = user.Apellido,
                    correo = user.Correo,
                    proceso = user.Proceso,
                    rol = rolActivo?.Nombre ?? "Sin rol",
                    permisos
                });
            })
            .RequireAuthorization();

            // 3. Solicitar restablecimiento de contraseña
            group.MapPost("/forgot-password", async (
                [FromBody] ForgotPasswordRequest request,
                [FromServices] IPasswordResetService passwordResetService,
                [FromServices] IEmailService emailService,
                [FromServices] IConfiguration configuration,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Identifier))
                {
                    return Results.BadRequest(new { message = "El correo o usuario es obligatorio." });
                }

                var result = await passwordResetService.CreateResetRequestAsync(request.Identifier, cancellationToken);
                if (!result.UserFound)
                {
                    return Results.Ok(new
                    {
                        message = "Si la cuenta existe, recibirás un enlace para restablecer tu contraseña."
                    });
                }

                var frontendUrl = configuration["Frontend:BaseUrl"];
                if (string.IsNullOrWhiteSpace(frontendUrl))
                {
                    return Results.Problem("No está configurada la URL del frontend.");
                }

                var resetUrl = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(result.Token!)}";

                try
                {
                    await emailService.SendPasswordResetEmailAsync(result.Correo!, resetUrl, cancellationToken);
                }
                catch
                {
                    return Results.Problem("No fue posible enviar el correo de recuperación.");
                }

                return Results.Ok(new
                {
                    message = "Si la cuenta existe, recibirás un enlace para restablecer tu contraseña."
                });
            })
            .RequireRateLimiting("auth-limit");

            // 4. Restablecer contraseña
            group.MapPost("/reset-password", async (
                [FromBody] ResetPasswordRequest request,
                [FromServices] IPasswordResetService passwordResetService,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return Results.BadRequest(new { message = "El token y la nueva contraseña son obligatorios." });
                }

                if (request.NewPassword.Length < 10 ||
                    !request.NewPassword.Any(char.IsUpper) ||
                    !request.NewPassword.Any(char.IsLower) ||
                    !request.NewPassword.Any(char.IsDigit) ||
                    !request.NewPassword.Any(c => !char.IsLetterOrDigit(c)))
                {
                    return Results.BadRequest(new { message = "La contraseña debe tener al menos 10 caracteres, incluir mayúsculas, minúsculas, números y un carácter especial." });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return Results.BadRequest(new { message = "Las contraseñas no coinciden." });
                }

                var result = await passwordResetService.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);

                if (result.IsExpired)
                {
                    return Results.BadRequest(new { message = "El enlace de recuperación ha expirado." });
                }

                if (!result.Successful)
                {
                    return Results.BadRequest(new { message = "El enlace de recuperación no es válido." });
                }

                return Results.Ok(new { message = "La contraseña fue restablecida correctamente." });
            })
            .RequireRateLimiting("auth-limit");

            // 5. Cambiar contraseña del usuario autenticado
            group.MapPost("/change-password", async (
                [FromBody] ChangePasswordRequest request,
                HttpContext httpContext,
                [FromServices] PlataformaActasDbContext context,
                CancellationToken cancellationToken) =>
            {
                var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return Results.BadRequest(new { message = "Todos los campos son obligatorios." });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return Results.BadRequest(new { message = "La nueva contraseña y su confirmación no coinciden." });
                }

                if (request.NewPassword.Length < 8 ||
                    !request.NewPassword.Any(char.IsUpper) ||
                    !request.NewPassword.Any(char.IsLower) ||
                    !request.NewPassword.Any(char.IsDigit) ||
                    !request.NewPassword.Any(c => !char.IsLetterOrDigit(c)))
                {
                    return Results.BadRequest(new { message = "La nueva contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y un carácter especial." });
                }

                var user = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                if (user is null || !user.Activo)
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return Results.BadRequest(new { message = "La contraseña actual ingresada es incorrecta." });
                }

                if (string.Equals(request.NewPassword, request.CurrentPassword, StringComparison.Ordinal) ||
                    BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                {
                    return Results.BadRequest(new { message = "La nueva contraseña debe ser diferente a la contraseña actual." });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await context.SaveChangesAsync(cancellationToken);

                return Results.Ok(new { message = "Tu contraseña ha sido actualizada exitosamente." });
            })
            .RequireAuthorization();

            return app;
        }
    }
}

namespace PlataformaActas.Api.Modules.Auth
{
    using PlataformaActas.Api.Modules.Auth.Endpoints;
    using PlataformaActas.Api.Modules.Auth.Services;

    public static class AuthModule
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }

        public static IEndpointRouteBuilder MapAuthModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapAuthEndpoints();
            return endpoints;
        }
    }
}
