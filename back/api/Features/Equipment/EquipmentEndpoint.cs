using PlataformaActas.Api.Authorization;

namespace PlataformaActas.Api.Features.Equipment;

public static class EquipmentEndpoint
{
    public static IEndpointRouteBuilder MapEquipmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        // =====================================================
        // LISTAR EQUIPOS
        // =====================================================

        endpoints.MapGet(
            "/api/equipment",
            async (
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var equipment = await service.GetAllAsync(cancellationToken);
                return Results.Ok(equipment);
            })
            .RequirePermission(Permissions.EquiposConsultar);

        // =====================================================
        // REGISTRAR EQUIPO
        // =====================================================

        endpoints.MapPost(
            "/api/equipment",
            async (
                CreateEquipmentRequest request,
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var equipment = await service.CreateAsync(request, cancellationToken);
                    return Results.Created($"/api/equipment/{equipment.Id}", equipment);
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

        // =====================================================
        // EDITAR EQUIPO
        // =====================================================

        endpoints.MapPut(
            "/api/equipment/{id:guid}",
            async (
                Guid id,
                UpdateEquipmentRequest request,
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var equipment = await service.UpdateAsync(id, request, cancellationToken);
                    if (equipment is null)
                    {
                        return Results.NotFound(new { message = "No se encontró el equipo solicitado." });
                    }
                    return Results.Ok(equipment);
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

        // =====================================================
        // DESACTIVAR EQUIPO
        // =====================================================

        endpoints.MapPatch(
            "/api/equipment/{id:guid}/deactivate",
            async (
                Guid id,
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var deactivated = await service.DeactivateAsync(id, cancellationToken);
                if (!deactivated)
                {
                    return Results.NotFound(new { message = "No se encontró el equipo solicitado." });
                }
                return Results.Ok(new { message = "El equipo fue desactivado correctamente." });
            })
            .RequirePermission(Permissions.EquiposDesactivar);

        // =====================================================
        // ACTIVAR EQUIPO
        // =====================================================

        endpoints.MapPatch(
            "/api/equipment/{id:guid}/activate",
            async (
                Guid id,
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var activated = await service.ActivateAsync(id, cancellationToken);
                if (!activated)
                {
                    return Results.NotFound(new { message = "No se encontró el equipo solicitado." });
                }
                return Results.Ok(new { message = "El equipo fue activado correctamente." });
            })
            .RequirePermission(Permissions.EquiposEditar);

        // =====================================================
        // ELIMINAR EQUIPO (INACTIVOS)
        // =====================================================

        endpoints.MapDelete(
            "/api/equipment/{id:guid}",
            async (
                Guid id,
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var deleted = await service.DeleteAsync(id, cancellationToken);
                    if (!deleted)
                    {
                        return Results.NotFound(new { message = "No se encontró el equipo solicitado." });
                    }
                    return Results.Ok(new { message = "El equipo fue eliminado del sistema exitosamente." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.EquiposDesactivar);

        // =====================================================
        // LISTAR MANTENIMIENTOS
        // =====================================================

        endpoints.MapGet(
            "/api/equipment/mantenimientos",
            async (
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var records = await service.GetMaintenanceListAsync(cancellationToken);
                return Results.Ok(records);
            })
            .RequirePermission(Permissions.EquiposConsultar);

        // =====================================================
        // ACTUALIZAR MANTENIMIENTO
        // =====================================================

        endpoints.MapPatch(
            "/api/equipment/{id:guid}/mantenimiento",
            async (
                Guid id,
                UpdateMaintenanceRequest request,
                EquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var updated = await service.UpdateMaintenanceAsync(id, request, cancellationToken);
                if (!updated)
                {
                    return Results.NotFound(new { message = "No se encontró el equipo solicitado." });
                }
                return Results.Ok(new { message = "Mantenimiento registrado correctamente." });
            })
            .RequirePermission(Permissions.EquiposEditar);

        return endpoints;
    }
}