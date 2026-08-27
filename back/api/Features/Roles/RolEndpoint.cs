using Npgsql;

using PlataformaActas.Api.Authorization;

namespace PlataformaActas.Api.Features.Roles;

public static class RolEndpoint
{
    public static IEndpointRouteBuilder MapRolEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        // =====================================================
        // OBTENER TODOS LOS ROLES
        // =====================================================

        endpoints.MapGet(
            "/api/roles",
            async (
                RolService service,
                CancellationToken cancellationToken) =>
            {
                var roles =
                    await service.GetAllAsync(
                        cancellationToken);

                return Results.Ok(roles);
            })
            .RequirePermission(
                Permissions.RolesConsultar);


        // =====================================================
        // OBTENER ROL POR ID
        // =====================================================

        endpoints.MapGet(
            "/api/roles/{id:int}",
            async (
                int id,
                RolService service,
                CancellationToken cancellationToken) =>
            {
                var rol =
                    await service.GetByIdAsync(
                        id,
                        cancellationToken);

                return rol is null
                    ? Results.NotFound()
                    : Results.Ok(rol);
            })
            .RequirePermission(
                Permissions.RolesConsultar);


        // =====================================================
        // CREAR ROL
        // =====================================================

        endpoints.MapPost(
            "/api/roles",
            async (
                CrearRolRequest request,
                RolService service,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(
                        request.Nombre))
                {
                    return Results.BadRequest(
                        new
                        {
                            mensaje =
                                "El nombre del rol es obligatorio."
                        });
                }

                try
                {
                    var rol =
                        await service.CreateAsync(
                            request,
                            cancellationToken);

                    return Results.Created(
                        $"/api/roles/{rol.Id}",
                        rol);
                }
                catch (PostgresException ex)
                    when (ex.SqlState == "23505")
                {
                    return Results.Conflict(
                        new
                        {
                            mensaje =
                                "Ya existe un rol con ese nombre."
                        });
                }
            })
            .RequirePermission(
                Permissions.RolesCrear);


        // =====================================================
        // ACTUALIZAR ROL
        // =====================================================

        endpoints.MapPut(
            "/api/roles/{id:int}",
            async (
                int id,
                ActualizarRolRequest request,
                RolService service,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(
                        request.Nombre))
                {
                    return Results.BadRequest(
                        new
                        {
                            mensaje =
                                "El nombre del rol es obligatorio."
                        });
                }

                try
                {
                    var rol =
                        await service.UpdateAsync(
                            id,
                            request,
                            cancellationToken);

                    return rol is null
                        ? Results.NotFound()
                        : Results.Ok(rol);
                }
                catch (PostgresException ex)
                    when (ex.SqlState == "23505")
                {
                    return Results.Conflict(
                        new
                        {
                            mensaje =
                                "Ya existe un rol con ese nombre."
                        });
                }
            })
            .RequirePermission(
                Permissions.RolesEditar);


        // =====================================================
        // ACTIVAR / DESACTIVAR ROL
        // =====================================================

        endpoints.MapPatch(
            "/api/roles/{id:int}/estado",
            async (
                int id,
                bool activo,
                RolService service,
                CancellationToken cancellationToken) =>
            {
                var actualizado =
                    await service.SetActiveAsync(
                        id,
                        activo,
                        cancellationToken);

                return actualizado
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .RequirePermission(
                Permissions.RolesEditar);


        // =====================================================
        // ELIMINAR ROL
        // =====================================================

        endpoints.MapDelete(
            "/api/roles/{id:int}",
            async (
                int id,
                RolService service,
                CancellationToken cancellationToken) =>
            {
                var resultado =
                    await service.DeleteAsync(
                        id,
                        cancellationToken);

                return resultado switch
                {
                    EliminarRolResultado.Eliminado =>
                        Results.NoContent(),

                    EliminarRolResultado.NoEncontrado =>
                        Results.NotFound(
                            new
                            {
                                mensaje =
                                    "El rol no existe."
                            }),

                    EliminarRolResultado.TieneUsuarios =>
                        Results.Conflict(
                            new
                            {
                                mensaje =
                                    "No se puede eliminar este rol "
                                    + "porque tiene usuarios asignados. "
                                    + "Desactívelo en su lugar."
                            }),

                    EliminarRolResultado.AdministradorProtegido =>
                        Results.Conflict(
                            new
                            {
                                mensaje =
                                    "El rol Administrador está protegido "
                                    + "y no puede eliminarse."
                            }),

                    _ =>
                        Results.Conflict(
                            new
                            {
                                mensaje =
                                    "No fue posible eliminar el rol."
                            })
                };
            })
            .RequirePermission(
                Permissions.RolesEliminar);


        // =====================================================
        // OBTENER PERMISOS
        // =====================================================

        endpoints.MapGet(
            "/api/permisos",
            async (
                RolService service,
                CancellationToken cancellationToken) =>
            {
                var permisos =
                    await service.GetPermisosAsync(
                        cancellationToken);

                return Results.Ok(permisos);
            })
            .RequirePermission(
                Permissions.RolesConsultar);


        // =====================================================
        // ACTUALIZAR PERMISOS DE UN ROL
        // =====================================================

        endpoints.MapPut(
            "/api/roles/{id:int}/permisos",
            async (
                int id,
                ActualizarPermisosRolRequest request,
                RolService service,
                CancellationToken cancellationToken) =>
            {
                var actualizado =
                    await service.SetPermisosAsync(
                        id,
                        request.PermisoIds,
                        cancellationToken);

                return actualizado
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .RequirePermission(
                Permissions.RolesEditar);


        return endpoints;
    }
}