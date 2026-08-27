using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PlataformaActas.Api.Modules.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () =>
            Results.Ok(new
            {
                status = "ok",
                service = "plataforma-actas-api",
                timestamp = DateTimeOffset.UtcNow
            }));

        return endpoints;
    }
}
