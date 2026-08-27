using Npgsql;

namespace PlataformaActas.Api.Features.Usuarios;

public sealed class UsuarioService(
    NpgsqlDataSource dataSource)
{
    // =========================================================
    // OBTENER TODOS LOS USUARIOS
    // =========================================================

    public async Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u."Id",
                u."Nombre",
                u."Apellido",
                u."UsuarioRed",
                u."Correo",
                u."Proceso",
                u."Activo",
                r."Id" AS "RolId",
                r."Nombre" AS "RolNombre"
            FROM usuarios u
            LEFT JOIN usuario_roles ur
                ON ur."UsuarioId" = u."Id"
            LEFT JOIN roles r
                ON r."Id" = ur."RolId"
                AND r."Activo" = TRUE
            ORDER BY
                u."Nombre",
                u."Apellido";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var usuarios =
            new List<UsuarioResponse>();

        while (
            await reader.ReadAsync(
                cancellationToken))
        {
            usuarios.Add(
                MapUsuario(reader)
            );
        }

        return usuarios;
    }


    // =========================================================
    // OBTENER USUARIO POR ID
    // =========================================================

    public async Task<UsuarioResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u."Id",
                u."Nombre",
                u."Apellido",
                u."UsuarioRed",
                u."Correo",
                u."Proceso",
                u."Activo",
                r."Id" AS "RolId",
                r."Nombre" AS "RolNombre"
            FROM usuarios u
            LEFT JOIN usuario_roles ur
                ON ur."UsuarioId" = u."Id"
            LEFT JOIN roles r
                ON r."Id" = ur."RolId"
                AND r."Activo" = TRUE
            WHERE
                u."Id" = @id
            LIMIT 1;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "id",
            id);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
                cancellationToken))
        {
            return null;
        }

        return MapUsuario(reader);
    }


    // =========================================================
    // CREAR USUARIO
    // =========================================================

    public async Task<UsuarioResponse> CreateAsync(
        UsuarioCreateRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO usuarios
            (
                "Nombre",
                "Apellido",
                "UsuarioRed",
                "Correo",
                "Proceso",
                "Activo",
                "FechaCreacion"
            )
            VALUES
            (
                @nombre,
                @apellido,
                @usuarioRed,
                @correo,
                @proceso,
                TRUE,
                CURRENT_TIMESTAMP
            )
            RETURNING
                "Id";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "nombre",
            request.Nombre.Trim());

        command.Parameters.AddWithValue(
            "apellido",
            request.Apellido.Trim());

        command.Parameters.AddWithValue(
            "usuarioRed",
            request.UsuarioRed.Trim());

        command.Parameters.AddWithValue(
            "correo",
            request.Correo.Trim());

        command.Parameters.AddWithValue(
            "proceso",
            request.Proceso.Trim());

        var result =
            await command.ExecuteScalarAsync(
                cancellationToken);

        var id =
            Convert.ToInt32(result);

        var usuario =
            await GetByIdAsync(
                id,
                cancellationToken);

        if (usuario is null)
        {
            throw new InvalidOperationException(
                "El usuario fue creado, pero no pudo ser recuperado.");
        }

        return usuario;
    }


    // =========================================================
    // ACTUALIZAR USUARIO
    // =========================================================

    public async Task<UsuarioResponse?> UpdateAsync(
        int id,
        UsuarioUpdateRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE usuarios
            SET
                "Nombre" = @nombre,
                "Apellido" = @apellido,
                "UsuarioRed" = @usuarioRed,
                "Correo" = @correo,
                "Proceso" = @proceso,
                "Activo" = @activo
            WHERE
                "Id" = @id;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "id",
            id);

        command.Parameters.AddWithValue(
            "nombre",
            request.Nombre.Trim());

        command.Parameters.AddWithValue(
            "apellido",
            request.Apellido.Trim());

        command.Parameters.AddWithValue(
            "usuarioRed",
            request.UsuarioRed.Trim());

        command.Parameters.AddWithValue(
            "correo",
            request.Correo.Trim());

        command.Parameters.AddWithValue(
            "proceso",
            request.Proceso.Trim());

        command.Parameters.AddWithValue(
            "activo",
            request.Activo);

        var affected =
            await command.ExecuteNonQueryAsync(
                cancellationToken);

        if (affected == 0)
        {
            return null;
        }

        return await GetByIdAsync(
            id,
            cancellationToken);
    }


    // =========================================================
    // DESACTIVAR USUARIO
    // =========================================================

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE usuarios
            SET
                "Activo" = FALSE
            WHERE
                "Id" = @id;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "id",
            id);

        var affected =
            await command.ExecuteNonQueryAsync(
                cancellationToken);

        return affected > 0;
    }


    // =========================================================
    // ASIGNAR / CAMBIAR ROL
    // =========================================================

    public async Task<UsuarioResponse?> AssignRoleAsync(
        int usuarioId,
        int rolId,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // -------------------------------------------------
            // VALIDAR USUARIO
            // -------------------------------------------------

            const string usuarioSql = """
                SELECT
                    "Id"
                FROM usuarios
                WHERE
                    "Id" = @usuarioId
                LIMIT 1;
                """;

            await using var usuarioCommand =
                new NpgsqlCommand(
                    usuarioSql,
                    connection,
                    transaction);

            usuarioCommand.Parameters.AddWithValue(
                "usuarioId",
                usuarioId);

            var usuarioExiste =
                await usuarioCommand.ExecuteScalarAsync(
                    cancellationToken);

            if (usuarioExiste is null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return null;
            }


            // -------------------------------------------------
            // VALIDAR ROL ACTIVO
            // -------------------------------------------------

            const string rolSql = """
                SELECT
                    "Id"
                FROM roles
                WHERE
                    "Id" = @rolId
                    AND "Activo" = TRUE
                LIMIT 1;
                """;

            await using var rolCommand =
                new NpgsqlCommand(
                    rolSql,
                    connection,
                    transaction);

            rolCommand.Parameters.AddWithValue(
                "rolId",
                rolId);

            var rolExiste =
                await rolCommand.ExecuteScalarAsync(
                    cancellationToken);

            if (rolExiste is null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                throw new InvalidOperationException(
                    "El rol seleccionado no existe o está inactivo.");
            }


            // -------------------------------------------------
            // ELIMINAR ROL ACTUAL
            // -------------------------------------------------

            const string deleteSql = """
                DELETE FROM usuario_roles
                WHERE
                    "UsuarioId" = @usuarioId;
                """;

            await using var deleteCommand =
                new NpgsqlCommand(
                    deleteSql,
                    connection,
                    transaction);

            deleteCommand.Parameters.AddWithValue(
                "usuarioId",
                usuarioId);

            await deleteCommand.ExecuteNonQueryAsync(
                cancellationToken);


            // -------------------------------------------------
            // ASIGNAR NUEVO ROL
            // -------------------------------------------------

            const string insertSql = """
                INSERT INTO usuario_roles
                (
                    "UsuarioId",
                    "RolId"
                )
                VALUES
                (
                    @usuarioId,
                    @rolId
                );
                """;

            await using var insertCommand =
                new NpgsqlCommand(
                    insertSql,
                    connection,
                    transaction);

            insertCommand.Parameters.AddWithValue(
                "usuarioId",
                usuarioId);

            insertCommand.Parameters.AddWithValue(
                "rolId",
                rolId);

            await insertCommand.ExecuteNonQueryAsync(
                cancellationToken);


            // -------------------------------------------------
            // CONFIRMAR TRANSACCIÓN
            // -------------------------------------------------

            await transaction.CommitAsync(
                cancellationToken);


            // -------------------------------------------------
            // DEVOLVER USUARIO ACTUALIZADO
            // -------------------------------------------------

            return await GetByIdAsync(
                usuarioId,
                cancellationToken);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(
                    cancellationToken);
            }
            catch
            {
                // La excepción original es la importante.
            }

            throw;
        }
    }


    // =========================================================
    // MAPEAR USUARIO
    // =========================================================

    private static UsuarioResponse MapUsuario(
        NpgsqlDataReader reader)
    {
        var idOrdinal =
            reader.GetOrdinal("Id");

        var nombreOrdinal =
            reader.GetOrdinal("Nombre");

        var apellidoOrdinal =
            reader.GetOrdinal("Apellido");

        var usuarioRedOrdinal =
            reader.GetOrdinal("UsuarioRed");

        var correoOrdinal =
            reader.GetOrdinal("Correo");

        var procesoOrdinal =
            reader.GetOrdinal("Proceso");

        var activoOrdinal =
            reader.GetOrdinal("Activo");

        var rolIdOrdinal =
            reader.GetOrdinal("RolId");

        var rolNombreOrdinal =
            reader.GetOrdinal("RolNombre");

        return new UsuarioResponse(
            reader.GetInt32(
                idOrdinal),

            reader.GetString(
                nombreOrdinal),

            reader.GetString(
                apellidoOrdinal),

            reader.GetString(
                usuarioRedOrdinal),

            reader.GetString(
                correoOrdinal),

            reader.GetString(
                procesoOrdinal),

            reader.GetBoolean(
                activoOrdinal),

            reader.IsDBNull(
                rolIdOrdinal)
                ? null
                : reader.GetInt32(
                    rolIdOrdinal),

            reader.IsDBNull(
                rolNombreOrdinal)
                ? null
                : reader.GetString(
                    rolNombreOrdinal)
        );
    }
}