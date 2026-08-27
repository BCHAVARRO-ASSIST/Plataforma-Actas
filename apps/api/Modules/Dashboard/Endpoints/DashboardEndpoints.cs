using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Modules.Dashboard.Services;

namespace PlataformaActas.Api.Modules.Dashboard.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/dashboard");

        group.MapGet("/kpis", async (
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var kpis = await service.GetKpisAsync(cancellationToken);
            return Results.Ok(kpis);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        group.MapGet("/charts/equipment-status", async (
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var chart = await service.GetEquipmentStatusChartAsync(cancellationToken);
            return Results.Ok(chart);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        group.MapGet("/charts/actas-status", async (
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var chart = await service.GetActasStatusChartAsync(cancellationToken);
            return Results.Ok(chart);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        group.MapGet("/charts/deliveries-messenger", async (
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var chart = await service.GetDeliveriesByMessengerAsync(cancellationToken);
            return Results.Ok(chart);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        group.MapGet("/charts/timeline", async (
            string? range,
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var timeline = await service.GetTimelineAsync(range, cancellationToken);
            return Results.Ok(timeline);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        group.MapGet("/maintenance-stats", async (
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var stats = await service.GetMaintenanceStatsAsync(cancellationToken);
            return Results.Ok(stats);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        group.MapGet("/recent-activity", async (
            int? limit,
            IDashboardService service,
            CancellationToken cancellationToken) =>
        {
            var activity = await service.GetRecentActivityAsync(limit ?? 10, cancellationToken);
            return Results.Ok(activity);
        })
        .RequirePermission(Permissions.DashboardConsultar);

        return endpoints;
    }
}
