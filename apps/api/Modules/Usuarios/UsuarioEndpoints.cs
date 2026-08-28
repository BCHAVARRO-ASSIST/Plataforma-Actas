using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
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
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var ok = await service.DeleteAsync(id, cancellationToken);
                return ok ? Results.NoContent() : Results.NotFound();
            })
            .RequirePermission(Permissions.UsuariosDesactivar);

            // 6. Asignar rol a usuario
            endpoints.MapPut("/api/usuarios/{id:int}/rol", async (
                int id,
                UsuarioRolRequest request,
                IUsuarioService service,
                CancellationToken cancellationToken) =>
            {
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
