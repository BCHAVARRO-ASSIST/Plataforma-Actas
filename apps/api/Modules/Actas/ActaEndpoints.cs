using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Common.Security;
using PlataformaActas.Api.Infrastructure.Security;
using PlataformaActas.Api.Modules.Actas.Data;
using PlataformaActas.Api.Modules.Actas.Endpoints;
using PlataformaActas.Api.Modules.Actas.Models.Dto;
using PlataformaActas.Api.Modules.Actas.Services;

namespace PlataformaActas.Api.Modules.Actas.Endpoints
{
    public static class ActaEndpoints
    {
        public static IEndpointRouteBuilder MapActaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/actas");

            // 1. Listar todas las actas
            group.MapGet("/", async (
                HttpContext context,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var actas = await actaService.GetAllAsync(cancellationToken);
                return Results.Ok(actas);
            })
            .RequirePermission(Permissions.ActasConsultar);

            // 2. Listar mis actas (para vista de mensajero)
            group.MapGet("/mis-actas", async (
                HttpContext httpContext,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var user = httpContext.User;
                var hasAdminAccess = user.Claims.Any(c => c.Type == "permiso" && c.Value == Permissions.ActasConsultar);

                if (hasAdminAccess)
                {
                    var allActas = await actaService.GetAllAsync(cancellationToken);
                    return Results.Ok(allActas);
                }

                var actas = await actaService.GetByMensajeroAsync(userId, cancellationToken);
                return Results.Ok(actas);
            })
            .RequireAnyPermission(Permissions.ActasConsultar, Permissions.MensajeroActasConsultar);

            // 3. Obtener detalle de acta
            group.MapGet("/{id:guid}", async (
                Guid id,
                HttpContext context,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null) return Results.NotFound();

                var canAccess = PermissionRequirementHandler.CanAccessMensajeroResource(
                    context.User,
                    acta.MensajeroUsuarioId);

                if (!canAccess)
                {
                    return Results.Forbid();
                }

                return Results.Ok(acta);
            })
            .RequireAuthorization();

            // 4. Crear acta
            group.MapPost("/", async (
                CreateActaRequest request,
                HttpContext httpContext,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                if (InputSanitizationValidator.ContainsSqlInjection(request.ObservacionesEntrega) ||
                    InputSanitizationValidator.ContainsSqlInjection(request.ObservacionesDevolucion))
                {
                    return Results.BadRequest(new { message = "Se detectaron caracteres o patrones maliciosos no permitidos en las observaciones del acta." });
                }

                try
                {
                    int creatorUserId = request.UsuarioId;
                    var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdStr, out var authUserId) && authUserId > 0)
                    {
                        creatorUserId = authUserId;
                    }

                    if (creatorUserId <= 0)
                    {
                        return Results.BadRequest(new { message = "No se pudo identificar el usuario autenticado que registra el acta." });
                    }

                    var safeRequest = request with { UsuarioId = creatorUserId };
                    var created = await actaService.CreateAsync(safeRequest, cancellationToken);
                    return Results.Created($"/api/actas/{created.Id}", created);
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
            .RequirePermission(Permissions.ActasRegistrar);

            // 5. Firmar acta (permite administrador o mensajero asignado)
            group.MapPost("/{id:guid}/firmar", async (
                Guid id,
                FirmarActaRequest request,
                HttpContext context,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null) return Results.NotFound();

                var canAccess = PermissionRequirementHandler.CanAccessMensajeroResource(
                    context.User,
                    acta.MensajeroUsuarioId,
                    adminPermission: Permissions.ActasFirmar,
                    mensajeroPermission: Permissions.MensajeroActasFirmar);

                if (!canAccess) return Results.Forbid();

                try
                {
                    var updated = await actaService.SignAsync(id, request, cancellationToken);
                    return updated is null ? Results.NotFound() : Results.Ok(updated);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequireAuthorization();

            // 6. Entregar acta (para mensajero)
            group.MapPost("/{id:guid}/entregar", async (
                Guid id,
                HttpContext context,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null) return Results.NotFound();

                var canAccess = PermissionRequirementHandler.CanAccessMensajeroResource(
                    context.User,
                    acta.MensajeroUsuarioId,
                    adminPermission: Permissions.ActasEditar,
                    mensajeroPermission: Permissions.MensajeroActasEntregar);

                if (!canAccess) return Results.Forbid();

                try
                {
                    var updated = await actaService.FinalizeAsync(id, cancellationToken);
                    return updated is null ? Results.NotFound() : Results.Ok(updated);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequireAuthorization();

            // 7. Devolver acta (para mensajero)
            group.MapPost("/{id:guid}/devolver", async (
                Guid id,
                AnularActaRequest request,
                HttpContext context,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null) return Results.NotFound();

                var canAccess = PermissionRequirementHandler.CanAccessMensajeroResource(
                    context.User,
                    acta.MensajeroUsuarioId,
                    adminPermission: Permissions.ActasEditar,
                    mensajeroPermission: Permissions.MensajeroActasDevolver);

                if (!canAccess) return Results.Forbid();

                try
                {
                    var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    int? usuarioId = int.TryParse(userIdStr, out var parsed) ? parsed : null;

                    var updated = await actaService.AnularAsync(id, request, usuarioId, cancellationToken);
                    return updated is null ? Results.NotFound() : Results.Ok(updated);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequireAuthorization();

            // 8. Finalizar acta (admin)
            group.MapPost("/{id:guid}/finalizar", async (
                Guid id,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var updated = await actaService.FinalizeAsync(id, cancellationToken);
                    return updated is null ? Results.NotFound() : Results.Ok(updated);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.ActasEditar);

            // 9. Anular acta (admin)
            group.MapPost("/{id:guid}/anular", async (
                Guid id,
                AnularActaRequest request,
                HttpContext httpContext,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                if (InputSanitizationValidator.ContainsSqlInjection(request.Motivo))
                {
                    return Results.BadRequest(new { message = "Se detectaron caracteres o patrones maliciosos no permitidos en el motivo de anulación." });
                }

                try
                {
                    var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    int? usuarioId = int.TryParse(userIdStr, out var parsed) ? parsed : null;

                    var updated = await actaService.AnularAsync(id, request, usuarioId, cancellationToken);
                    return updated is null ? Results.NotFound() : Results.Ok(updated);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequirePermission(Permissions.ActasEditar);

            // 10. Visualizar PDF inline
            group.MapGet("/{id:guid}/pdf", async (
                Guid id,
                IDocumentoActaService documentoService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var doc = await documentoService.ObtenerOVisualizarPdfAsync(id, cancellationToken);
                    return Results.File(doc.PdfBytes, doc.ContentType, enableRangeProcessing: true);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .RequirePermission(Permissions.ActasConsultar);

            // 11. Descargar archivo PDF
            group.MapGet("/{id:guid}/descargar-pdf", async (
                Guid id,
                IDocumentoActaService documentoService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var doc = await documentoService.ObtenerOVisualizarPdfAsync(id, cancellationToken);
                    return Results.File(doc.PdfBytes, doc.ContentType, fileDownloadName: doc.FileName);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .RequirePermission(Permissions.ActasConsultar);

            // 12. Regenerar PDF
            group.MapPost("/{id:guid}/generar-pdf", async (
                Guid id,
                IDocumentoActaService documentoService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var response = await documentoService.GenerarYGuardarPdfAsync(id, cancellationToken);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .RequirePermission(Permissions.ActasEditar);

            return endpoints;
        }
    }
}

namespace PlataformaActas.Api.Modules.Actas
{
    public static class ActaModule
    {
        public static IServiceCollection AddActaModule(this IServiceCollection services)
        {
            services.AddScoped<PlataformaActas.Api.Infrastructure.Storage.IFileStorageProvider, PlataformaActas.Api.Infrastructure.Storage.LocalFileStorageProvider>();
            services.AddScoped<IActaRepository, ActaRepository>();
            services.AddScoped<IActaService, ActaService>();
            services.AddScoped<IDocumentoActaService, DocumentoActaService>();
            return services;
        }

        public static IEndpointRouteBuilder MapActaModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapActaEndpoints();
            return endpoints;
        }
    }
}
