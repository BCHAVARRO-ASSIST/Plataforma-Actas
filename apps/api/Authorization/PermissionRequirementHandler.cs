using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PlataformaActas.Api.Authorization;

public static class PermissionRequirementHandler
{
    /// <summary>
    /// Checks whether the current user is an admin or if the resource belongs to the courier (mensajero).
    /// Prevents Insecure Direct Object References (IDOR).
    /// </summary>
    public static bool CanAccessMensajeroResource(
        ClaimsPrincipal user,
        int? mensajeroUsuarioId,
        string adminPermission = Permissions.ActasConsultar,
        string mensajeroPermission = Permissions.MensajeroActasConsultar)
    {
        var hasAdminPermission = user.Claims.Any(c =>
            c.Type == "permiso" &&
            string.Equals(c.Value, adminPermission, StringComparison.OrdinalIgnoreCase));

        if (hasAdminPermission)
        {
            return true;
        }

        var hasMensajeroPermission = user.Claims.Any(c =>
            c.Type == "permiso" &&
            string.Equals(c.Value, mensajeroPermission, StringComparison.OrdinalIgnoreCase));

        if (!hasMensajeroPermission)
        {
            return false;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        return mensajeroUsuarioId.HasValue && mensajeroUsuarioId.Value == userId;
    }
}
