using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Dashboard.Endpoints;
using PlataformaActas.Api.Modules.Dashboard.Services;

namespace PlataformaActas.Api.Modules.Dashboard;

public static class DashboardModule
{
    public static IServiceCollection AddDashboardModule(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }

    public static IEndpointRouteBuilder MapDashboardModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDashboardEndpoints();
        return endpoints;
    }
}
