using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Modules.Equipos.Data;
using PlataformaActas.Api.Modules.Equipos.Models.Dto;
using PlataformaActas.Api.Modules.Equipos.Services;

namespace PlataformaActas.Api.Modules.Equipos.Endpoints;

public static class EquiposEndpoints
{
    public static IEndpointRouteBuilder MapEquiposEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // 1. Grupo Canónico Principal: /api/equipos
        var equiposGroup = endpoints.MapGroup("/api/equipos");
        MapEquipoRoutes(equiposGroup);

        // 2. Grupo Legado en Deprecación: /api/equipment
        // Emite cabecera HTTP estándar RFC 7234 (Warning: 299) informando obsolescencia del endpoint
        var legacyGroup = endpoints.MapGroup("/api/equipment")
            .AddEndpointFilter(async (context, next) =>
            {
                context.HttpContext.Response.Headers.Append("Warning", "299 - \"Ruta obsoleta. Utilice /api/equipos\"");
                return await next(context);
            });
        MapEquipoRoutes(legacyGroup);

        return endpoints;
    }

    private static void MapEquipoRoutes(RouteGroupBuilder group)
    {
        // 1. Listar equipos
        group.MapGet("", async (
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            var equipment = await service.GetAllAsync(cancellationToken);
            return Results.Ok(equipment);
        })
        .RequirePermission(Permissions.EquiposConsultar);

        // 2. Registrar equipo
        group.MapPost("", async (
            CreateEquipmentRequest request,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/equipos/{created.Id}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.EquiposRegistrar);

        // 3. Editar equipo
        group.MapPut("{id:guid}", async (
            Guid id,
            UpdateEquipmentRequest request,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, request, cancellationToken);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.EquiposEditar);

        // 4. Detalle de equipo
        group.MapGet("{id:guid}", async (
            Guid id,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            var eq = await service.GetByIdAsync(id, cancellationToken);
            return eq is null ? Results.NotFound() : Results.Ok(eq);
        })
        .RequirePermission(Permissions.EquiposConsultar);

        // 5. Desactivar equipo (cambiar a Inactivo)
        group.MapPatch("{id:guid}/deactivate", async (
            Guid id,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var ok = await service.DeactivateAsync(id, cancellationToken);
                return ok ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.EquiposDesactivar);

        // 6. Activar equipo
        group.MapPatch("{id:guid}/activate", async (
            Guid id,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            var ok = await service.ActivateAsync(id, cancellationToken);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .RequirePermission(Permissions.EquiposEditar);

        // 7. Eliminar equipo físico
        group.MapDelete("{id:guid}", async (
            Guid id,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
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
        .RequirePermission(Permissions.EquiposDesactivar);

        // 8. Listar mantenimientos
        group.MapGet("mantenimientos", async (
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            var list = await service.GetMaintenanceListAsync(cancellationToken);
            return Results.Ok(list);
        })
        .RequirePermission(Permissions.EquiposConsultar);

        // 9. Actualizar mantenimiento
        group.MapPut("{id:guid}/mantenimiento", async (
            Guid id,
            UpdateMaintenanceRequest request,
            IEquiposService service,
            CancellationToken cancellationToken) =>
        {
            var ok = await service.UpdateMaintenanceAsync(id, request, cancellationToken);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .RequirePermission(Permissions.EquiposEditar);
    }
}
