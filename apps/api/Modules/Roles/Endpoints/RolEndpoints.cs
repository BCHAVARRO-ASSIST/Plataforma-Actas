using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Modules.Roles.Models.Dto;
using PlataformaActas.Api.Modules.Roles.Services;

namespace PlataformaActas.Api.Modules.Roles.Endpoints;

public static class RolEndpoints
{
    public static IEndpointRouteBuilder MapRolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // 1. Obtener todos los roles
        endpoints.MapGet("/api/roles", async (
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var roles = await service.GetAllAsync(cancellationToken);
            return Results.Ok(roles);
        })
        .RequirePermission(Permissions.RolesConsultar);

        // 2. Obtener detalle de un rol
        endpoints.MapGet("/api/roles/{id:int}", async (
            int id,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var rol = await service.GetByIdAsync(id, cancellationToken);
            return rol is null ? Results.NotFound() : Results.Ok(rol);
        })
        .RequirePermission(Permissions.RolesConsultar);

        // 3. Crear nuevo rol
        endpoints.MapPost("/api/roles", async (
            CrearRolRequest request,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return Results.BadRequest(new { message = "El nombre del rol es obligatorio." });
            }

            try
            {
                var rol = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/roles/{rol.Id}", rol);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.RolesCrear);

        // 4. Actualizar rol
        endpoints.MapPut("/api/roles/{id:int}", async (
            int id,
            ActualizarRolRequest request,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return Results.BadRequest(new { message = "El nombre del rol es obligatorio." });
            }

            try
            {
                var rol = await service.UpdateAsync(id, request, cancellationToken);
                return rol is null ? Results.NotFound() : Results.Ok(rol);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.RolesEditar);

        // 5. Activar rol
        endpoints.MapPatch("/api/roles/{id:int}/activar", async (
            int id,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var ok = await service.SetActiveAsync(id, true, cancellationToken);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .RequirePermission(Permissions.RolesEditar);

        // 6. Desactivar rol
        endpoints.MapPatch("/api/roles/{id:int}/desactivar", async (
            int id,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var ok = await service.SetActiveAsync(id, false, cancellationToken);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .RequirePermission(Permissions.RolesEditar);

        // 7. Eliminar rol
        endpoints.MapDelete("/api/roles/{id:int}", async (
            int id,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.DeleteAsync(id, cancellationToken);

            return result switch
            {
                EliminarRolResultado.Eliminado => Results.NoContent(),
                EliminarRolResultado.NoEncontrado => Results.NotFound(),
                EliminarRolResultado.TieneUsuarios => Results.Conflict(new
                {
                    message = "No es posible eliminar el rol porque tiene usuarios asignados."
                }),
                EliminarRolResultado.AdministradorProtegido => Results.Conflict(new
                {
                    message = "El rol 'Administrador' no puede ser eliminado por ser crítico para el sistema."
                }),
                _ => Results.StatusCode(500)
            };
        })
        .RequirePermission(Permissions.RolesEliminar);

        // 8. Obtener catálogo de permisos
        endpoints.MapGet("/api/roles/permisos", async (
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var permisos = await service.GetPermisosAsync(cancellationToken);
            return Results.Ok(permisos);
        })
        .RequirePermission(Permissions.RolesConsultar);

        // 9. Actualizar permisos de un rol
        endpoints.MapPut("/api/roles/{id:int}/permisos", async (
            int id,
            ActualizarPermisosRolRequest request,
            IRolService service,
            CancellationToken cancellationToken) =>
        {
            var ok = await service.SetPermisosAsync(id, request.PermisoIds ?? Array.Empty<int>(), cancellationToken);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .RequirePermission(Permissions.RolesEditar);

        return endpoints;
    }
}
