using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Reportes.Endpoints;
using PlataformaActas.Api.Modules.Reportes.Services;

namespace PlataformaActas.Api.Modules.Reportes;

public static class ReportesModule
{
    public static IServiceCollection AddReportesModule(this IServiceCollection services)
    {
        services.AddScoped<IReportesService, ReportesService>();
        return services;
    }

    public static IEndpointRouteBuilder MapReportesModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapReportesEndpoints();
        return endpoints;
    }
}
