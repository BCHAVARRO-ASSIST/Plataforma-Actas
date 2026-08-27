using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PlataformaActas.Api.Features.Health;

public static class HealthEndpoint
{
    public static IEndpointRouteBuilder MapHealthEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () =>
            Results.Ok(new
            {
                status = "ok",
                service = "plataforma-actas-api"
            }));

        return endpoints;
    }
}