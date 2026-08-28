using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Modules.Equipment.Data;
using PlataformaActas.Api.Modules.Equipment.Endpoints;
using PlataformaActas.Api.Modules.Equipment.Models.Dto;
using PlataformaActas.Api.Modules.Equipment.Services;

namespace PlataformaActas.Api.Modules.Equipment.Endpoints
{
    public static class EquipmentEndpoints
    {
        public static IEndpointRouteBuilder MapEquipmentEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // 1. Listar equipos
            endpoints.MapGet("/api/equipment", async (
                IEquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var equipment = await service.GetAllAsync(cancellationToken);
                return Results.Ok(equipment);
            })
            .RequirePermission(Permissions.EquiposConsultar);

            // 2. Registrar equipo
            endpoints.MapPost("/api/equipment", async (
                CreateEquipmentRequest request,
                IEquipmentService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var created = await service.CreateAsync(request, cancellationToken);
                    return Results.Created($"/api/equipment/{created.Id}", created);
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
            endpoints.MapPut("/api/equipment/{id:guid}", async (
                Guid id,
                UpdateEquipmentRequest request,
                IEquipmentService service,
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

            // 4. Desactivar equipo
            endpoints.MapPatch("/api/equipment/{id:guid}/deactivate", async (
                Guid id,
                IEquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var ok = await service.DeactivateAsync(id, cancellationToken);
                return ok ? Results.NoContent() : Results.NotFound();
            })
            .RequirePermission(Permissions.EquiposDesactivar);

            // 5. Activar equipo
            endpoints.MapPatch("/api/equipment/{id:guid}/activate", async (
                Guid id,
                IEquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var ok = await service.ActivateAsync(id, cancellationToken);
                return ok ? Results.NoContent() : Results.NotFound();
            })
            .RequirePermission(Permissions.EquiposEditar);

            // 6. Eliminar equipo definitivamente
            endpoints.MapDelete("/api/equipment/{id:guid}", async (
                Guid id,
                IEquipmentService service,
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

            // 7. Listar mantenimientos
            endpoints.MapGet("/api/equipment/mantenimientos", async (
                IEquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var list = await service.GetMaintenanceListAsync(cancellationToken);
                return Results.Ok(list);
            })
            .RequirePermission(Permissions.EquiposConsultar);

            // 8. Actualizar mantenimiento
            endpoints.MapPut("/api/equipment/{id:guid}/mantenimiento", async (
                Guid id,
                UpdateMaintenanceRequest request,
                IEquipmentService service,
                CancellationToken cancellationToken) =>
            {
                var ok = await service.UpdateMaintenanceAsync(id, request, cancellationToken);
                return ok ? Results.NoContent() : Results.NotFound();
            })
            .RequirePermission(Permissions.EquiposEditar);

            return endpoints;
        }
    }
}

namespace PlataformaActas.Api.Modules.Equipment
{
    public static class EquipmentModule
    {
        public static IServiceCollection AddEquipmentModule(this IServiceCollection services)
        {
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IEquipmentService, EquipmentService>();
            return services;
        }

        public static IEndpointRouteBuilder MapEquipmentModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapEquipmentEndpoints();
            return endpoints;
        }
    }
}
