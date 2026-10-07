using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Modules.Auth.Endpoints;
using PlataformaActas.Api.Modules.Auth.Models.Dto;
using PlataformaActas.Api.Modules.Integrations.Google;

namespace PlataformaActas.Api.Modules.Integrations;

public sealed record GoogleAuthRequest(string? IdToken);
public sealed record GoogleDriveSyncRequest(string? DriveFileId = null, string? SheetName = null);

public static class IntegrationsEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/integrations");

        // 1. Sincronización manual bajo demanda desde Google Drive
        group.MapPost("/google-drive/sync", async (
            [FromBody] GoogleDriveSyncRequest? request,
            IGoogleDriveSyncService syncService,
            CancellationToken cancellationToken) =>
        {
            var result = await syncService.SyncInventoryAsync(request?.DriveFileId, request?.SheetName, cancellationToken);
            if (result.Success)
            {
                return Results.Ok(result);
            }

            var statusCode = result.ErrorCode switch
            {
                "GOOGLE_DRIVE_DISABLED" => StatusCodes.Status412PreconditionFailed,
                "GOOGLE_CREDENTIALS_MISSING" => StatusCodes.Status412PreconditionFailed,
                "GOOGLE_DRIVE_FILE_NOT_FOUND" => StatusCodes.Status412PreconditionFailed,
                "GOOGLE_DRIVE_PERMISSION_DENIED" => StatusCodes.Status412PreconditionFailed,
                "INVALID_DRIVE_FILE_ID" => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status422UnprocessableEntity
            };

            return Results.Problem(
                statusCode: statusCode,
                title: "Error de sincronización con Google Drive",
                detail: result.Message,
                type: statusCode == StatusCodes.Status412PreconditionFailed 
                    ? "https://tools.ietf.org/html/rfc7232#section-4.2" 
                    : statusCode == StatusCodes.Status400BadRequest
                        ? "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                        : "https://tools.ietf.org/html/rfc4918#section-11.2",
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = result.ErrorCode ?? "SYNC_FAILED",
                    ["success"] = false,
                    ["timestamp"] = result.Timestamp
                }
            );
        })
        .RequireAnyPermission(Permissions.EquiposRegistrar, Permissions.EquiposEditar);

        // 2. Estado de la última sincronización
        group.MapGet("/google-drive/status", (IGoogleDriveSyncService syncService) =>
        {
            var status = syncService.GetLastSyncStatus();
            return status != null
                ? Results.Ok(status)
                : Results.Ok(new { message = "Aún no se ha ejecutado ninguna sincronización en esta sesión." });
        })
        .RequirePermission(Permissions.EquiposConsultar);

        // 3. Verificación de usuario en Directorio de Google
        group.MapGet("/google-directory/user/{email}", async (
            string email,
            IGoogleDirectoryService directoryService,
            CancellationToken cancellationToken) =>
        {
            var result = await directoryService.ValidateUserAsync(email, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAnyPermission(Permissions.UsuariosConsultar, Permissions.UsuariosRegistrar);

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
                var expiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes) ? minutes : 480;
                AuthEndpoints.AppendSessionCookie(httpContext, loginResp.Token, expiresMinutes);
            }
            return result;
        })
        .RequireRateLimiting("auth-limit");

        return endpoints;
    }
}
