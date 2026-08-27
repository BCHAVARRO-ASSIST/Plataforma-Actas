using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Auth.Endpoints;
using PlataformaActas.Api.Modules.Auth.Services;

namespace PlataformaActas.Api.Modules.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }

    public static IEndpointRouteBuilder MapAuthModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        return endpoints;
    }
}
