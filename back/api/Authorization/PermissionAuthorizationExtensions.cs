using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PlataformaActas.Api.Authorization;

public static class PermissionAuthorizationExtensions
{
    // =====================================================
    // REQUIERE UN PERMISO
    // =====================================================

    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder endpoint,
        string permission)
    {
        endpoint.RequireAuthorization();

        endpoint.AddEndpointFilter(
            async (context, next) =>
            {
                var tienePermiso =
                    context.HttpContext.User.Claims.Any(
                        claim =>
                            claim.Type == "permiso" &&
                            string.Equals(
                                claim.Value,
                                permission,
                                StringComparison.OrdinalIgnoreCase));

                if (!tienePermiso)
                {
                    return Results.Forbid();
                }

                return await next(context);
            });

        return endpoint;
    }


    // =====================================================
    // REQUIERE AL MENOS UNO DE LOS PERMISOS
    // =====================================================

    public static RouteHandlerBuilder RequireAnyPermission(
        this RouteHandlerBuilder endpoint,
        params string[] permissions)
    {
        endpoint.RequireAuthorization();

        endpoint.AddEndpointFilter(
            async (context, next) =>
            {
                var permisosUsuario =
                    context.HttpContext.User.Claims
                        .Where(
                            claim =>
                                claim.Type == "permiso")
                        .Select(
                            claim =>
                                claim.Value)
                        .ToHashSet(
                            StringComparer.OrdinalIgnoreCase);


                var tienePermiso =
                    permissions.Any(
                        permission =>
                            permisosUsuario.Contains(
                                permission));


                if (!tienePermiso)
                {
                    return Results.Forbid();
                }

                return await next(context);
            });

        return endpoint;
    }
}