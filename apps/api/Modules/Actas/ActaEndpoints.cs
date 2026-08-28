using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
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

            // 1. Listar actas (con filtro por mensajero si aplica)
            group.MapGet("/", async (
                HttpContext context,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var user = context.User;
                var isMensajero = user.IsInRole("Mensajero") || user.IsInRole("Repartidor");

                if (isMensajero)
                {
                    var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdStr, out var mensajeroId))
                    {
                        var mensajeroActas = await actaService.GetByMensajeroAsync(mensajeroId, cancellationToken);
                        return Results.Ok(mensajeroActas);
                    }
                }

                var actas = await actaService.GetAllAsync(cancellationToken);
                return Results.Ok(actas);
            })
            .RequirePermission(Permissions.ActasConsultar);

            // 2. Obtener detalle de acta
            group.MapGet("/{id:guid}", async (
                Guid id,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                return acta is null ? Results.NotFound() : Results.Ok(acta);
            })
            .RequirePermission(Permissions.ActasConsultar);

            // 3. Crear acta
            group.MapPost("/", async (
                CreateActaRequest request,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var created = await actaService.CreateAsync(request, cancellationToken);
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

            // 4. Firmar acta
            group.MapPost("/{id:guid}/firmar", async (
                Guid id,
                FirmarActaRequest request,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
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
            .RequirePermission(Permissions.ActasFirmar);

            // 5. Finalizar acta
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

            // 6. Anular acta
            group.MapPost("/{id:guid}/anular", async (
                Guid id,
                AnularActaRequest request,
                HttpContext httpContext,
                IActaService actaService,
                CancellationToken cancellationToken) =>
            {
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

            // 7. Visualizar PDF inline
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

            // 8. Descargar archivo PDF
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

            // 9. Regenerar PDF
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
