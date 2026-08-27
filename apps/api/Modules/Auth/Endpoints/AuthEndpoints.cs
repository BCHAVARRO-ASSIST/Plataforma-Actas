using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using PlataformaActas.Api.Modules.Auth.Models.Dto;
using PlataformaActas.Api.Modules.Auth.Services;

namespace PlataformaActas.Api.Modules.Auth.Endpoints;

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
        });

        // 2. Usuario autenticado (me)
        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var usuarioId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuarioRed = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var correo = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var rol = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var permisos = httpContext.User.FindAll("permiso")
                .Select(c => c.Value)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            return Results.Ok(new
            {
                usuarioId,
                usuarioRed,
                correo,
                rol,
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
        });

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

            if (request.NewPassword.Length < 8)
            {
                return Results.BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres." });
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
        });

        return app;
    }
}
