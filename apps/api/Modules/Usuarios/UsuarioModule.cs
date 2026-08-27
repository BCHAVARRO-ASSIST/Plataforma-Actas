using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Usuarios.Data;
using PlataformaActas.Api.Modules.Usuarios.Endpoints;
using PlataformaActas.Api.Modules.Usuarios.Services;

namespace PlataformaActas.Api.Modules.Usuarios;

public static class UsuarioModule
{
    public static IServiceCollection AddUsuarioModule(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        return services;
    }

    public static IEndpointRouteBuilder MapUsuarioModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapUsuarioEndpoints();
        return endpoints;
    }
}
