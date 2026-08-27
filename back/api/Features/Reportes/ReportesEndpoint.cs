using PlataformaActas.Api.Authorization;

namespace PlataformaActas.Api.Features.Reportes;

public static class ReportesEndpoint
{
    public static IEndpointRouteBuilder MapReportesEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reportes");

        // =====================================================
        // REPORTE DE EQUIPOS
        // =====================================================

        group.MapPost(
            "/equipos",
            async (
                ReportFilterRequest filter,
                ReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetEquiposReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(
                Permissions.ReportesConsultar);

        // =====================================================
        // REPORTE DE ACTAS
        // =====================================================

        group.MapPost(
            "/actas",
            async (
                ReportFilterRequest filter,
                ReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetActasReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(
                Permissions.ReportesConsultar);

        // =====================================================
        // REPORTE DE ENTREGAS
        // =====================================================

        group.MapPost(
            "/entregas",
            async (
                ReportFilterRequest filter,
                ReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetEntregasReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(
                Permissions.ReportesConsultar);

        // =====================================================
        // REPORTE DE DEVOLUCIONES
        // =====================================================

        group.MapPost(
            "/devoluciones",
            async (
                ReportFilterRequest filter,
                ReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetDevolucionesReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(
                Permissions.ReportesConsultar);

        return endpoints;
    }
}
