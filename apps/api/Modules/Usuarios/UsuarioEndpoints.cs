using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Common.Security;
using PlataformaActas.Api.Modules.Usuarios.Data;
using PlataformaActas.Api.Modules.Usuarios.Endpoints;
using PlataformaActas.Api.Modules.Usuarios.Models.Dto;
using PlataformaActas.Api.Modules.Usuarios.Services;

namespace PlataformaActas.Api.Modules.Usuarios.Endpoints
{
    public static class UsuarioEndpoints
    {
        public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // 1. Obtener todos los usuarios
            endpoints.MapGet("/api/usuarios", async (
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var usuarios = await service.GetAllAsync(cancellationToken);
                return Results.Ok(usuarios);
            })
            .RequirePermission(Permissions.UsuariosConsultar);

            // 2. Obtener usuario por ID
            endpoints.MapGet("/api/usuarios/{id:int}", async (
                int id,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var usuario = await service.GetByIdAsync(id, cancellationToken);
                return usuario is null ? Results.NotFound() : Results.Ok(usuario);
            })
            .RequirePermission(Permissions.UsuariosConsultar);

            // 3. Crear usuario
            endpoints.MapPost("/api/usuarios", async (
                UsuarioCreateRequest request,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    string.IsNullOrWhiteSpace(request.Apellido) ||
                    string.IsNullOrWhiteSpace(request.UsuarioRed) ||
                    string.IsNullOrWhiteSpace(request.Correo) ||
                    string.IsNullOrWhiteSpace(request.Proceso))
                {
                    return Results.BadRequest(new { message = "Todos los campos obligatorios deben ser diligenciados." });
                }

                if (!System.Net.Mail.MailAddress.TryCreate(request.Correo.Trim(), out var parsedMail) || parsedMail.Address != request.Correo.Trim())
                {
                    return Results.BadRequest(new { message = "El formato del correo electrónico es inválido." });
                }

                if (request.Nombre.Length > 100 || request.Apellido.Length > 100 || request.UsuarioRed.Length > 50 || request.Correo.Length > 150)
                {
                    return Results.BadRequest(new { message = "Uno o más campos exceden la longitud máxima permitida." });
                }

                // Prevención estricta de SQL Injection en datos de entrada
                if (InputSanitizationValidator.ContainsSqlInjection(request.Nombre) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.Apellido) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.UsuarioRed) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.Proceso) ||
                    !InputSanitizationValidator.IsValidDocumentNumber(request.NumeroDocumento))
                {
                    return Results.BadRequest(new { message = "Se detectaron caracteres o patrones maliciosos no permitidos en los datos de entrada." });
                }

                try
                {
                    var usuario = await service.CreateAsync(request, cancellationToken);
                    return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.UsuariosRegistrar);

            // 4. Actualizar usuario
            endpoints.MapPut("/api/usuarios/{id:int}", async (
                int id,
                UsuarioUpdateRequest request,
                HttpContext httpContext,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var currentUserIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.FindFirst("sub")?.Value;
                if (!request.Activo && int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
                {
                    return Results.BadRequest(new { message = "No puedes desactivar o revocar tu propia cuenta de administrador en sesión." });
                }

                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    string.IsNullOrWhiteSpace(request.Apellido) ||
                    string.IsNullOrWhiteSpace(request.UsuarioRed) ||
                    string.IsNullOrWhiteSpace(request.Correo) ||
                    string.IsNullOrWhiteSpace(request.Proceso))
                {
                    return Results.BadRequest(new { message = "Todos los campos obligatorios deben ser diligenciados." });
                }

                if (!System.Net.Mail.MailAddress.TryCreate(request.Correo.Trim(), out var parsedMail) || parsedMail.Address != request.Correo.Trim())
                {
                    return Results.BadRequest(new { message = "El formato del correo electrónico es inválido." });
                }

                if (request.Nombre.Length > 100 || request.Apellido.Length > 100 || request.UsuarioRed.Length > 50 || request.Correo.Length > 150)
                {
                    return Results.BadRequest(new { message = "Uno o más campos exceden la longitud máxima permitida." });
                }

                // Prevención estricta de SQL Injection en actualización de usuario
                if (InputSanitizationValidator.ContainsSqlInjection(request.Nombre) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.Apellido) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.UsuarioRed) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.Proceso) ||
                    !InputSanitizationValidator.IsValidDocumentNumber(request.NumeroDocumento))
                {
                    return Results.BadRequest(new { message = "Se detectaron caracteres o patrones maliciosos no permitidos en los datos de entrada." });
                }

                try
                {
                    var usuario = await service.UpdateAsync(id, request, cancellationToken);
                    return usuario is null ? Results.NotFound() : Results.Ok(usuario);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.UsuariosEditar);

            // 5. Desactivar usuario
            endpoints.MapDelete("/api/usuarios/{id:int}", async (
                int id,
                HttpContext httpContext,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var currentUserIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.FindFirst("sub")?.Value;
                if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
                {
                    return Results.BadRequest(new { message = "No puedes desactivar o revocar tu propia cuenta de administrador en sesión." });
                }

                try
                {
                    var ok = await service.DeleteAsync(id, cancellationToken);
                    return ok ? Results.NoContent() : Results.NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.UsuariosDesactivar);

            // 6. Asignar rol a usuario
            endpoints.MapPut("/api/usuarios/{id:int}/rol", async (
                int id,
                UsuarioRolRequest request,
                HttpContext httpContext,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var currentUserIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.FindFirst("sub")?.Value;
                if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
                {
                    return Results.BadRequest(new { message = "No puedes modificar tu propio rol en sesión." });
                }

                try
                {
                    var usuario = await service.AssignRoleAsync(id, request.RolId, cancellationToken);
                    return usuario is null ? Results.NotFound() : Results.Ok(usuario);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.UsuariosEditar);

            // 7. Restablecer contraseña de usuario (Administrador)
            endpoints.MapPut("/api/usuarios/{id:int}/password", async (
                int id,
                UsuarioResetPasswordRequest request,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return Results.BadRequest(new { message = "La nueva contraseña y su confirmación son obligatorias." });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return Results.BadRequest(new { message = "Las contraseñas ingresadas no coinciden." });
                }

                if (request.NewPassword.Length < 8 ||
                    !request.NewPassword.Any(char.IsUpper) ||
                    !request.NewPassword.Any(char.IsLower) ||
                    !request.NewPassword.Any(char.IsDigit) ||
                    !request.NewPassword.Any(c => !char.IsLetterOrDigit(c)))
                {
                    return Results.BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y un carácter especial." });
                }

                var ok = await service.ResetPasswordAsync(id, request.NewPassword, cancellationToken);
                return ok ? Results.Ok(new { message = "Contraseña actualizada exitosamente." }) : Results.NotFound();
            })
            .RequirePermission(Permissions.UsuariosEditar);

            return endpoints;
        }
    }
}

namespace PlataformaActas.Api.Modules.Usuarios
{
    public static class UsuarioModule
    {
        public static IServiceCollection AddUsuarioModule(this IServiceCollection services)
        {
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            return services;
        }

        public static IEndpointRouteBuilder MapUsuarioModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapUsuarioEndpoints();
            return endpoints;
        }
    }
}
