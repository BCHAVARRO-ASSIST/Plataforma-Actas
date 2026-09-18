using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Equipos.Data;
using PlataformaActas.Api.Modules.Equipos.Endpoints;
using PlataformaActas.Api.Modules.Equipos.Services;

namespace PlataformaActas.Api.Modules.Equipos;

public static class EquiposModule
{
    public static IServiceCollection AddEquiposModule(this IServiceCollection services)
    {
        services.AddScoped<IEquiposRepository, EquiposRepository>();
        services.AddScoped<IEquiposService, EquiposService>();
        return services;
    }

    public static IEndpointRouteBuilder MapEquiposModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapEquiposEndpoints();
        return endpoints;
    }
}
