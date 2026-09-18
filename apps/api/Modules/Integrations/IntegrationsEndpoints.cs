using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using PlataformaActas.Api.Modules.Auth.Endpoints;
using PlataformaActas.Api.Modules.Auth.Models.Dto;
using PlataformaActas.Api.Modules.Integrations.Google;

namespace PlataformaActas.Api.Modules.Integrations;

public sealed record GoogleAuthRequest(string? IdToken);

public static class IntegrationsEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/integrations");

        // 1. Sincronización manual bajo demanda desde Google Drive
        group.MapPost("/google-drive/sync", async (
            IGoogleDriveSyncService syncService,
            CancellationToken cancellationToken) =>
        {
            var result = await syncService.SyncInventoryAsync(cancellationToken);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();

        // 2. Estado de la última sincronización
        group.MapGet("/google-drive/status", (IGoogleDriveSyncService syncService) =>
        {
            var status = syncService.GetLastSyncStatus();
            return status != null
                ? Results.Ok(status)
                : Results.Ok(new { message = "Aún no se ha ejecutado ninguna sincronización en esta sesión." });
        })
        .RequireAuthorization();

        // 3. Verificación de usuario en Directorio de Google
        group.MapGet("/google-directory/user/{email}", async (
            string email,
            IGoogleDirectoryService directoryService,
            CancellationToken cancellationToken) =>
        {
            var result = await directoryService.ValidateUserAsync(email, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        // 4. Autenticación Oficial con Google OAuth (Google Identity Services)
        endpoints.MapPost("/api/auth/google", async (
            [FromBody] GoogleAuthRequest request,
            HttpContext httpContext,
            [FromServices] IGoogleAuthService googleAuthService,
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var result = await googleAuthService.AuthenticateWithGoogleAsync(request?.IdToken, cancellationToken);
            if (result is Microsoft.AspNetCore.Http.HttpResults.Ok<LoginResponse> okResult && okResult.Value is { Token: not null } loginResp)
            {
                var expiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes) ? minutes : 30;
                AuthEndpoints.AppendSessionCookie(httpContext, loginResp.Token, expiresMinutes);
            }
            return result;
        });

        return endpoints;
    }
}
