using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Infrastructure.Storage;
using PlataformaActas.Api.Modules.Actas.Data;
using PlataformaActas.Api.Modules.Actas.Endpoints;
using PlataformaActas.Api.Modules.Actas.Services;

namespace PlataformaActas.Api.Modules.Actas;

public static class ActaModule
{
    public static IServiceCollection AddActaModule(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();
        services.AddScoped<IActaRepository, ActaRepository>();
        services.AddScoped<IDocumentoActaService, DocumentoActaService>();
        services.AddScoped<IActaService, ActaService>();
        return services;
    }

    public static IEndpointRouteBuilder MapActaModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapActaEndpoints();
        return endpoints;
    }
}
