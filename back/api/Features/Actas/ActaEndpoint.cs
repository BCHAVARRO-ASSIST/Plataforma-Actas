using System.Security.Claims;

using PlataformaActas.Api.Authorization;

namespace PlataformaActas.Api.Features.Actas;

public static class ActaEndpoint
{
    public static IEndpointRouteBuilder MapActaEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        // =====================================================
        // LISTAR TODAS LAS ACTAS
        // =====================================================

        endpoints.MapGet(
            "/api/actas",
            async (
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                var actas =
                    await service.GetAllAsync(
                        cancellationToken);

                return Results.Ok(actas);
            })
            .RequirePermission(
                Permissions.ActasConsultar);


        // =====================================================
        // LISTAR MIS ACTAS COMO MENSAJERO
        // =====================================================

        endpoints.MapGet(
            "/api/actas/mis-actas",
            async (
                HttpContext httpContext,
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                var usuarioIdClaim =
                    httpContext.User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(
                    usuarioIdClaim))
                {
                    return Results.Unauthorized();
                }

                if (!int.TryParse(
                    usuarioIdClaim,
                    out var usuarioId))
                {
                    return Results.Unauthorized();
                }

                var actas =
                    await service.GetByMensajeroAsync(
                        usuarioId,
                        cancellationToken);

                return Results.Ok(actas);
            })
            .RequirePermission(
                Permissions.MensajeroActasConsultar);


        // =====================================================
        // OBTENER DETALLE DE ACTA
        // =====================================================

        endpoints.MapGet(
            "/api/actas/{id:guid}",
            async (
                Guid id,
                HttpContext httpContext,
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                var acta =
                    await service.GetDetailByIdAsync(
                        id,
                        cancellationToken);

                if (acta is null)
                {
                    return Results.NotFound();
                }

                var tienePermisoActas =
                    httpContext.User.Claims.Any(
                        claim =>
                            claim.Type == "permiso" &&
                            string.Equals(
                                claim.Value,
                                Permissions.ActasConsultar,
                                StringComparison.OrdinalIgnoreCase));

                if (tienePermisoActas)
                {
                    return Results.Ok(acta);
                }

                var tienePermisoMensajero =
                    httpContext.User.Claims.Any(
                        claim =>
                            claim.Type == "permiso" &&
                            string.Equals(
                                claim.Value,
                                Permissions.MensajeroActasConsultar,
                                StringComparison.OrdinalIgnoreCase));

                if (!tienePermisoMensajero)
                {
                    return Results.Forbid();
                }

                var usuarioIdClaim =
                    httpContext.User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(
                    usuarioIdClaim))
                {
                    return Results.Unauthorized();
                }

                if (!int.TryParse(
                    usuarioIdClaim,
                    out var usuarioId))
                {
                    return Results.Unauthorized();
                }

                if (
                    acta.MensajeroUsuarioId
                    != usuarioId)
                {
                    return Results.Forbid();
                }

                return Results.Ok(acta);
            })
            .RequireAnyPermission(
                Permissions.ActasConsultar,
                Permissions.MensajeroActasConsultar);


        // =====================================================
        // CREAR ACTA
        // =====================================================

        endpoints.MapPost(
            "/api/actas",
            async (
                CreateActaRequest request,
                HttpContext httpContext,
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                var usuarioIdClaim =
                    httpContext.User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(usuarioIdClaim)
                    || !int.TryParse(usuarioIdClaim, out var authUsuarioId))
                {
                    return Results.Unauthorized();
                }

                var secureRequest =
                    request with { UsuarioId = authUsuarioId };

                if (
                    !string.Equals(
                        secureRequest.TipoActa,
                        "Entrega",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    !string.Equals(
                        secureRequest.TipoActa,
                        "Devolución",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "El tipo de acta debe ser 'Entrega' o 'Devolución'."
                        });
                }

                if (
                    string.Equals(
                        secureRequest.TipoActa,
                        "Devolución",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    secureRequest.ActaOrigenId is null)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Una acta de devolución debe estar relacionada con un acta de entrega."
                        });
                }

                if (
                    string.Equals(
                        secureRequest.TipoActa,
                        "Entrega",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    secureRequest.ActaOrigenId is not null)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Una acta de entrega no debe tener un acta de origen."
                        });
                }

                try
                {
                    var acta =
                        await service.CreateAsync(
                            secureRequest,
                            cancellationToken);

                    return Results.Created(
                        $"/api/actas/{acta.Id}",
                        acta);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(
                        new
                        {
                            message = ex.Message
                        });
                }
            })
            .RequirePermission(
                Permissions.ActasRegistrar);


        // =====================================================
        // FIRMAR ACTA
        // =====================================================

        endpoints.MapPost(
            "/api/actas/{id:guid}/firmar",
            async (
                Guid id,
                FirmarActaRequest request,
                HttpContext httpContext,
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                if (
                    string.IsNullOrWhiteSpace(
                        request.SignatureDataJson))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "La firma manuscrita es obligatoria."
                        });
                }

                var actaExistente =
                    await service.GetDetailByIdAsync(
                        id,
                        cancellationToken);

                if (actaExistente is null)
                {
                    return Results.NotFound(
                        new
                        {
                            message =
                                "El acta no existe."
                        });
                }

                var tienePermisoAdmin =
                    httpContext.User.Claims.Any(
                        claim =>
                            claim.Type == "permiso" &&
                            string.Equals(
                                claim.Value,
                                Permissions.ActasFirmar,
                                StringComparison.OrdinalIgnoreCase));

                var tienePermisoMensajero =
                    httpContext.User.Claims.Any(
                        claim =>
                            claim.Type == "permiso" &&
                            string.Equals(
                                claim.Value,
                                Permissions.MensajeroActasFirmar,
                                StringComparison.OrdinalIgnoreCase));

                if (!tienePermisoAdmin && !tienePermisoMensajero)
                {
                    return Results.Forbid();
                }

                if (!tienePermisoAdmin && tienePermisoMensajero)
                {
                    var usuarioIdClaim =
                        httpContext.User.FindFirst(
                            ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrWhiteSpace(usuarioIdClaim)
                        || !int.TryParse(usuarioIdClaim, out var authUsuarioId))
                    {
                        return Results.Unauthorized();
                    }

                    if (actaExistente.MensajeroUsuarioId != authUsuarioId)
                    {
                        return Results.Forbid();
                    }
                }

                try
                {
                    var acta =
                        await service.FirmarAsync(
                            id,
                            request.SignatureDataJson,
                            cancellationToken);

                    if (acta is null)
                    {
                        return Results.NotFound(
                            new
                            {
                                message =
                                    "El acta no existe."
                            });
                    }

                    return Results.Ok(acta);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(
                        new
                        {
                            message = ex.Message
                        });
                }
            })
            .RequireAnyPermission(
                Permissions.ActasFirmar,
                Permissions.MensajeroActasFirmar);


        // =====================================================
        // FINALIZAR ACTA
        // =====================================================

        endpoints.MapPost(
            "/api/actas/{id:guid}/finalizar",
            async (
                Guid id,
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var acta =
                        await service.FinalizarAsync(
                            id,
                            cancellationToken);

                    if (acta is null)
                    {
                        return Results.NotFound(
                            new
                            {
                                message = "El acta no existe."
                            });
                    }

                    return Results.Ok(acta);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(
                        new
                        {
                            message = ex.Message
                        });
                }
            })
            .RequirePermission(
                Permissions.ActasEditar);


        // =====================================================
        // ANULAR ACTA
        // =====================================================

        endpoints.MapPost(
            "/api/actas/{id:guid}/anular",
            async (
                Guid id,
                AnularActaRequest request,
                ActaService service,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Motivo))
                {
                    return Results.BadRequest(
                        new
                        {
                            message = "El motivo de anulación es obligatorio."
                        });
                }

                try
                {
                    var acta =
                        await service.AnularAsync(
                            id,
                            request.Motivo,
                            cancellationToken);

                    if (acta is null)
                    {
                        return Results.NotFound(
                            new
                            {
                                message = "El acta no existe."
                            });
                    }

                    return Results.Ok(acta);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(
                        new
                        {
                            message = ex.Message
                        });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(
                        new
                        {
                            message = ex.Message
                        });
                }
            })
            .RequirePermission(
                Permissions.ActasEditar);


        // =====================================================
        // DOCUMENTO OFICIAL: VISTA PREVIA PDF
        // =====================================================

        endpoints.MapGet(
            "/api/actas/{id:guid}/documento/preview",
            async (
                Guid id,
                HttpContext httpContext,
                ActaService actaService,
                DocumentoActaService docService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null)
                {
                    return Results.NotFound(new { message = "El acta solicitada no existe." });
                }

                // Validación de autorización / IDOR
                var tienePermisoAdmin = httpContext.User.Claims.Any(c =>
                    c.Type == "permiso" &&
                    (string.Equals(c.Value, Permissions.ActasConsultar, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(c.Value, Permissions.ActasExportar, StringComparison.OrdinalIgnoreCase)));

                if (!tienePermisoAdmin)
                {
                    var usuarioIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrWhiteSpace(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                    {
                        return Results.Unauthorized();
                    }

                    if (acta.MensajeroUsuarioId != usuarioId)
                    {
                        return Results.Forbid();
                    }
                }

                var docResult = docService.GetPreview(acta);
                return Results.File(docResult.PdfBytes, docResult.ContentType);
            })
            .RequireAnyPermission(
                Permissions.ActasConsultar,
                Permissions.ActasExportar,
                Permissions.MensajeroActasConsultar);


        // =====================================================
        // DOCUMENTO OFICIAL: DESCARGAR PDF
        // =====================================================

        endpoints.MapGet(
            "/api/actas/{id:guid}/documento/descargar",
            async (
                Guid id,
                HttpContext httpContext,
                ActaService actaService,
                DocumentoActaService docService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null)
                {
                    return Results.NotFound(new { message = "El acta solicitada no existe." });
                }

                // Validación de autorización / IDOR
                var tienePermisoAdmin = httpContext.User.Claims.Any(c =>
                    c.Type == "permiso" &&
                    (string.Equals(c.Value, Permissions.ActasConsultar, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(c.Value, Permissions.ActasExportar, StringComparison.OrdinalIgnoreCase)));

                if (!tienePermisoAdmin)
                {
                    var usuarioIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrWhiteSpace(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                    {
                        return Results.Unauthorized();
                    }

                    if (acta.MensajeroUsuarioId != usuarioId)
                    {
                        return Results.Forbid();
                    }
                }

                var docResult = await docService.GetDescargaAsync(acta, cancellationToken);

                return Results.File(
                    docResult.PdfBytes,
                    docResult.ContentType,
                    fileDownloadName: docResult.FileName);
            })
            .RequireAnyPermission(
                Permissions.ActasConsultar,
                Permissions.ActasExportar,
                Permissions.MensajeroActasConsultar);


        // =====================================================
        // DOCUMENTO OFICIAL: GENERAR Y PERSISTIR
        // =====================================================

        endpoints.MapPost(
            "/api/actas/{id:guid}/documento/generar",
            async (
                Guid id,
                ActaService actaService,
                DocumentoActaService docService,
                CancellationToken cancellationToken) =>
            {
                var acta = await actaService.GetDetailByIdAsync(id, cancellationToken);
                if (acta is null)
                {
                    return Results.NotFound(new { message = "El acta no existe." });
                }

                var response = await docService.GenerarYGuardarDocumentoAsync(acta, cancellationToken);

                return Results.Ok(response);
            })
            .RequireAnyPermission(
                Permissions.ActasEditar,
                Permissions.ActasRegistrar,
                Permissions.ActasExportar);


        return endpoints;
    }
}