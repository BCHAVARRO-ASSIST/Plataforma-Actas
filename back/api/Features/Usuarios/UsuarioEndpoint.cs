using PlataformaActas.Api.Authorization;

namespace PlataformaActas.Api.Features.Usuarios;

public static class UsuarioEndpoint
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        // =====================================================
        // GET TODOS
        // =====================================================

        endpoints.MapGet(
            "/api/usuarios",
            async (
                UsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var usuarios =
                    await service.GetAllAsync(
                        cancellationToken);

                return Results.Ok(usuarios);
            })
            .RequirePermission(
                Permissions.UsuariosConsultar);


        // =====================================================
        // GET POR ID
        // =====================================================

        endpoints.MapGet(
            "/api/usuarios/{id:int}",
            async (
                int id,
                UsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var usuario =
                    await service.GetByIdAsync(
                        id,
                        cancellationToken);

                return usuario is null
                    ? Results.NotFound(
                        new
                        {
                            message =
                                "Usuario no encontrado."
                        })
                    : Results.Ok(usuario);
            })
            .RequirePermission(
                Permissions.UsuariosConsultar);


        // =====================================================
        // POST - CREAR
        // =====================================================

        endpoints.MapPost(
            "/api/usuarios",
            async (
                UsuarioCreateRequest request,
                UsuarioService service,
                CancellationToken cancellationToken) =>
            {
                if (
                    string.IsNullOrWhiteSpace(
                        request.Nombre) ||
                    string.IsNullOrWhiteSpace(
                        request.Apellido) ||
                    string.IsNullOrWhiteSpace(
                        request.UsuarioRed) ||
                    string.IsNullOrWhiteSpace(
                        request.Correo) ||
                    string.IsNullOrWhiteSpace(
                        request.Proceso))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Todos los campos son obligatorios."
                        });
                }

                try
                {
                    var usuario =
                        await service.CreateAsync(
                            request,
                            cancellationToken);

                    return Results.Created(
                        $"/api/usuarios/{usuario.Id}",
                        usuario);
                }
                catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
                {
                    return Results.Conflict(
                        new
                        {
                            message =
                                "Ya existe un usuario con el mismo usuario de red o correo electrónico."
                        });
                }
                catch (Exception)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "No fue posible crear el usuario."
                        });
                }
            })
            .RequirePermission(
                Permissions.UsuariosRegistrar);


        // =====================================================
        // PUT - ACTUALIZAR
        // =====================================================

        endpoints.MapPut(
            "/api/usuarios/{id:int}",
            async (
                int id,
                UsuarioUpdateRequest request,
                UsuarioService service,
                CancellationToken cancellationToken) =>
            {
                if (
                    string.IsNullOrWhiteSpace(
                        request.Nombre) ||
                    string.IsNullOrWhiteSpace(
                        request.Apellido) ||
                    string.IsNullOrWhiteSpace(
                        request.UsuarioRed) ||
                    string.IsNullOrWhiteSpace(
                        request.Correo) ||
                    string.IsNullOrWhiteSpace(
                        request.Proceso))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Todos los campos son obligatorios."
                        });
                }

                var usuario =
                    await service.UpdateAsync(
                        id,
                        request,
                        cancellationToken);

                return usuario is null
                    ? Results.NotFound(
                        new
                        {
                            message =
                                "Usuario no encontrado."
                        })
                    : Results.Ok(usuario);
            })
            .RequirePermission(
                Permissions.UsuariosEditar);


        // =====================================================
        // PUT - ASIGNAR / CAMBIAR ROL
        // =====================================================

        endpoints.MapPut(
            "/api/usuarios/{id:int}/rol",
            async (
                int id,
                UsuarioRolRequest request,
                UsuarioService service,
                CancellationToken cancellationToken) =>
            {
                if (request.RolId <= 0)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Debe seleccionar un rol válido."
                        });
                }

                try
                {
                    var usuario =
                        await service.AssignRoleAsync(
                            id,
                            request.RolId,
                            cancellationToken);

                    return usuario is null
                        ? Results.NotFound(
                            new
                            {
                                message =
                                    "Usuario no encontrado."
                            })
                        : Results.Ok(usuario);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                ex.Message
                        });
                }
            })
            .RequirePermission(
                Permissions.RolesEditar);



        // =====================================================
        // DELETE - DESACTIVAR
        // =====================================================

        endpoints.MapDelete(
            "/api/usuarios/{id:int}",
            async (
                int id,
                UsuarioService service,
                CancellationToken cancellationToken) =>
            {
                var eliminado =
                    await service.DeleteAsync(
                        id,
                        cancellationToken);

                return eliminado
                    ? Results.NoContent()
                    : Results.NotFound(
                        new
                        {
                            message =
                                "Usuario no encontrado."
                        });
            })
            .RequirePermission(
                Permissions.UsuariosDesactivar);


        return endpoints;
    }
}