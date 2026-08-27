using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace PlataformaActas.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");


        // =====================================================
        // LOGIN
        // =====================================================

        group.MapPost(
            "/login",
            async (
                [FromBody] LoginRequest request,
                [FromServices] AuthService authService,
                [FromServices] JwtTokenService jwtTokenService) =>
            {
                if (
                    string.IsNullOrWhiteSpace(request.Usuario) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "El usuario y la contraseña son obligatorios."
                        });
                }

                var login =
                    await authService.AuthenticateAsync(
                        request.Usuario,
                        request.Password);

                if (login is null)
                {
                    return Results.Unauthorized();
                }

                var token =
                    jwtTokenService.CreateToken(
                        login.Usuario,
                        login.Rol,
                        login.Permisos);

                return Results.Ok(
                    new
                    {
                        token,
                        usuario = login.Usuario,
                        rol = login.Rol,
                        permisos = login.Permisos
                    });
            });


        // =====================================================
        // USUARIO AUTENTICADO
        // =====================================================

        group.MapGet(
            "/me",
            (HttpContext httpContext) =>
            {
                var usuarioId =
                    httpContext.User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                var usuarioRed =
                    httpContext.User.FindFirst(
                        ClaimTypes.Name)?.Value;

                var nombre =
                    httpContext.User.FindFirst(
                        "nombre")?.Value;

                var apellido =
                    httpContext.User.FindFirst(
                        "apellido")?.Value;

                var correo =
                    httpContext.User.FindFirst(
                        ClaimTypes.Email)?.Value;

                var rol =
                    httpContext.User.FindFirst(
                        ClaimTypes.Role)?.Value;

                var permisos =
                    httpContext.User
                        .FindAll("permiso")
                        .Select(c => c.Value)
                        .Distinct()
                        .OrderBy(p => p)
                        .ToList();

                return Results.Ok(
                    new
                    {
                        usuarioId,
                        usuarioRed,
                        nombre,
                        apellido,
                        correo,
                        rol,
                        permisos
                    });
            })
            .RequireAuthorization();


        // =====================================================
        // SOLICITAR RESTABLECIMIENTO DE CONTRASEÑA
        // =====================================================

        group.MapPost(
            "/forgot-password",
            async (
                [FromBody] ForgotPasswordRequest request,
                [FromServices] PasswordResetService passwordResetService,
                [FromServices] EmailService emailService,
                [FromServices] IConfiguration configuration) =>
            {
                if (
                    string.IsNullOrWhiteSpace(
                        request.Identifier))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "El correo o usuario es obligatorio."
                        });
                }

                var result =
                    await passwordResetService
                        .CreateResetRequestAsync(
                            request.Identifier);

                // =================================================
                // RESPUESTA GENÉRICA
                // =================================================
                // No revelamos si el usuario existe.
                if (!result.UserFound)
                {
                    return Results.Ok(
                        new
                        {
                            message =
                                "Si la cuenta existe, recibirás un enlace para restablecer tu contraseña."
                        });
                }

                // =================================================
                // URL DEL FRONTEND
                // =================================================

                var frontendUrl =
                    configuration["Frontend:BaseUrl"];

                if (string.IsNullOrWhiteSpace(
                    frontendUrl))
                {
                    return Results.Problem(
                        "No está configurada la URL del frontend.");
                }

                var resetUrl =
                    $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(result.Token!)}";

                // =================================================
                // ENVÍO DEL CORREO
                // =================================================

                try
                {
                    await emailService
                        .SendPasswordResetEmailAsync(
                            result.Correo!,
                            resetUrl);
                }
                catch
                {
                    return Results.Problem(
                        "No fue posible enviar el correo de recuperación.");
                }

                // =================================================
                // RESPUESTA FINAL
                // =================================================
                // IMPORTANTE:
                // Ya NO devolvemos token, correo ni expiración.
                return Results.Ok(
                    new
                    {
                        message =
                            "Si la cuenta existe, recibirás un enlace para restablecer tu contraseña."
                    });
            });


        // =====================================================
        // RESTABLECER CONTRASEÑA
        // =====================================================

        group.MapPost(
            "/reset-password",
            async (
                [FromBody] ResetPasswordRequest request,
                [FromServices] PasswordResetService passwordResetService) =>
            {
                if (
                    string.IsNullOrWhiteSpace(
                        request.Token) ||
                    string.IsNullOrWhiteSpace(
                        request.NewPassword))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "El token y la nueva contraseña son obligatorios."
                        });
                }

                if (request.NewPassword.Length < 8)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "La contraseña debe tener al menos 8 caracteres."
                        });
                }

                if (
                    request.NewPassword !=
                    request.ConfirmPassword)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Las contraseñas no coinciden."
                        });
                }

                var result =
                    await passwordResetService
                        .ResetPasswordAsync(
                            request.Token,
                            request.NewPassword);

                // =================================================
                // TOKEN EXPIRADO
                // =================================================

                if (result.IsExpired)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "El enlace de recuperación ha expirado."
                        });
                }

                // =================================================
                // TOKEN INVÁLIDO
                // =================================================

                if (!result.Successful)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "El enlace de recuperación no es válido."
                        });
                }

                // =================================================
                // CONTRASEÑA CAMBIADA
                // =================================================

                return Results.Ok(
                    new
                    {
                        message =
                            "La contraseña fue restablecida correctamente."
                    });
            });


        return app;
    }
}