using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Integrations.Google;

namespace PlataformaActas.Api.Modules.Integrations;

public static class IntegrationsModuleExtensions
{
    public static IServiceCollection AddIntegrationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleIntegrationOptions>(configuration.GetSection(GoogleIntegrationOptions.SectionName));
        services.AddScoped<IGoogleDriveSyncService, GoogleDriveSyncService>();
        services.AddScoped<IGoogleDirectoryService, GoogleDirectoryService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddHostedService<GoogleInventorySyncBackgroundService>();

        return services;
    }

    public static IEndpointRouteBuilder MapIntegrationsModule(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapIntegrationsEndpoints();
    }
}
