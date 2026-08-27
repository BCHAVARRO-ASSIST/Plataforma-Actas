using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Roles.Data;
using PlataformaActas.Api.Modules.Roles.Endpoints;
using PlataformaActas.Api.Modules.Roles.Services;

namespace PlataformaActas.Api.Modules.Roles;

public static class RolModule
{
    public static IServiceCollection AddRolModule(this IServiceCollection services)
    {
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IRolService, RolService>();
        return services;
    }

    public static IEndpointRouteBuilder MapRolModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRolEndpoints();
        return endpoints;
    }
}
