using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Common.Security;
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
            var validationErrors = ValidateEquipment(
                request.InventoryKey,
                request.NombreEquipo,
                request.AsignadoA,
                request.Empresa,
                request.Proveedor,
                request.Marca,
                request.Procesador,
                request.DiscoDuro,
                request.MemoriaRam,
                request.Departamento);

            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors, statusCode: StatusCodes.Status400BadRequest);
            }

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
            var validationErrors = ValidateEquipment(
                request.InventoryKey,
                request.NombreEquipo,
                request.AsignadoA,
                request.Empresa,
                request.Proveedor,
                request.Marca,
                request.Procesador,
                request.DiscoDuro,
                request.MemoriaRam,
                request.Departamento);

            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors, statusCode: StatusCodes.Status400BadRequest);
            }

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
            var validationErrors = ValidateMaintenance(request);
            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors, statusCode: StatusCodes.Status400BadRequest);
            }

            var ok = await service.UpdateMaintenanceAsync(id, request, cancellationToken);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .RequirePermission(Permissions.EquiposEditar);
    }

    private static Dictionary<string, string[]> ValidateEquipment(
        string? inventoryKey,
        string? nombreEquipo,
        string? asignadoA,
        string? empresa,
        string? proveedor,
        string? marca,
        string? procesador,
        string? discoDuro,
        string? memoriaRam,
        string? departamento)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(inventoryKey))
        {
            errors["inventoryKey"] = ["La clave o placa de inventario es obligatoria."];
        }
        else if (!InputSanitizationValidator.IsValidInventoryKey(inventoryKey))
        {
            errors["inventoryKey"] = ["La placa de inventario debe ser alfanumérica limpia (entre 2 y 50 caracteres) sin patrones prohibidos."];
        }

        if (string.IsNullOrWhiteSpace(nombreEquipo))
        {
            errors["nombreEquipo"] = ["El nombre o modelo del equipo es obligatorio."];
        }
        else
        {
            var trimmed = nombreEquipo.Trim();
            if (trimmed.Length < 2 || trimmed.Length > 150 || InputSanitizationValidator.ContainsSqlInjection(trimmed))
            {
                errors["nombreEquipo"] = ["El nombre o modelo del equipo debe tener entre 2 y 150 caracteres y no contener patrones prohibidos."];
            }
        }

        if (!string.IsNullOrWhiteSpace(asignadoA))
        {
            var trimmed = asignadoA.Trim();
            if (trimmed.Length > 100 || InputSanitizationValidator.ContainsSqlInjection(trimmed))
            {
                errors["asignadoA"] = ["El nombre del responsable no debe exceder 100 caracteres ni contener caracteres prohibidos."];
            }
        }

        void CheckField(string fieldName, string? value, int maxLen)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var trimmed = value.Trim();
                if (trimmed.Length > maxLen || InputSanitizationValidator.ContainsSqlInjection(trimmed))
                {
                    errors[fieldName] = [$"El campo {fieldName} no debe exceder {maxLen} caracteres ni contener patrones prohibidos."];
                }
            }
        }

        CheckField("empresa", empresa, 100);
        CheckField("proveedor", proveedor, 100);
        CheckField("marca", marca, 100);
        CheckField("procesador", procesador, 100);
        CheckField("discoDuro", discoDuro, 100);
        CheckField("memoriaRam", memoriaRam, 100);
        CheckField("departamento", departamento, 100);

        return errors;
    }

    private static Dictionary<string, string[]> ValidateMaintenance(UpdateMaintenanceRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.TipoMantenimiento))
        {
            var tipo = request.TipoMantenimiento.Trim();
            if (!tipo.Equals("Preventivo", StringComparison.OrdinalIgnoreCase) &&
                !tipo.Equals("Correctivo", StringComparison.OrdinalIgnoreCase))
            {
                errors["tipoMantenimiento"] = ["El tipo de mantenimiento debe ser 'Preventivo' o 'Correctivo'."];
            }
        }

        if (request.FechaMantenimiento.HasValue)
        {
            var fecha = request.FechaMantenimiento.Value;
            var maxFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));
            var minPast = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
            if (fecha > maxFuture || fecha < minPast)
            {
                errors["fechaMantenimiento"] = ["La fecha de mantenimiento debe encontrarse en un rango temporal válido."];
            }
        }

        return errors;
    }
}
