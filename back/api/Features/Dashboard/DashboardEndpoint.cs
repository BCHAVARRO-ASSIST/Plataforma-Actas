using PlataformaActas.Api.Authorization;

namespace PlataformaActas.Api.Features.Dashboard;

public static class DashboardEndpoint
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/dashboard");

        // =====================================================
        // KPIS
        // =====================================================

        group.MapGet(
            "/kpis",
            async (
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var kpis = await service.GetKpisAsync(cancellationToken);
                return Results.Ok(kpis);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.ActasConsultar,
                Permissions.EquiposConsultar);

        // =====================================================
        // ESTADO DE EQUIPOS
        // =====================================================

        group.MapGet(
            "/equipment-status",
            async (
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var stats = await service.GetEquipmentStatusChartAsync(cancellationToken);
                return Results.Ok(stats);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.EquiposConsultar);

        // =====================================================
        // ESTADO DE ACTAS
        // =====================================================

        group.MapGet(
            "/actas-status",
            async (
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var stats = await service.GetActasStatusChartAsync(cancellationToken);
                return Results.Ok(stats);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.ActasConsultar);

        // =====================================================
        // ENTREGAS POR MENSAJERO
        // =====================================================

        group.MapGet(
            "/deliveries-by-messenger",
            async (
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var stats = await service.GetDeliveriesByMessengerAsync(cancellationToken);
                return Results.Ok(stats);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.ActasConsultar);

        // =====================================================
        // LÍNEA TEMPORAL
        // =====================================================

        group.MapGet(
            "/timeline",
            async (
                string? range,
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var stats = await service.GetTimelineAsync(range, cancellationToken);
                return Results.Ok(stats);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.ActasConsultar);

        // =====================================================
        // MANTENIMIENTOS
        // =====================================================

        group.MapGet(
            "/maintenance",
            async (
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var stats = await service.GetMaintenanceStatsAsync(cancellationToken);
                return Results.Ok(stats);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.EquiposConsultar);

        // =====================================================
        // ACTIVIDAD RECIENTE
        // =====================================================

        group.MapGet(
            "/recent-activity",
            async (
                int? limit,
                DashboardService service,
                CancellationToken cancellationToken) =>
            {
                var items = await service.GetRecentActivityAsync(limit ?? 10, cancellationToken);
                return Results.Ok(items);
            })
            .RequireAnyPermission(
                Permissions.DashboardConsultar,
                Permissions.ActasConsultar);

        return endpoints;
    }
}
