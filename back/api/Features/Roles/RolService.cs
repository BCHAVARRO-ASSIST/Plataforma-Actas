using Npgsql;

namespace PlataformaActas.Api.Features.Roles;

public enum EliminarRolResultado
{
    Eliminado,
    NoEncontrado,
    TieneUsuarios,
    AdministradorProtegido
}


public sealed class RolService(NpgsqlDataSource dataSource)
{
    public async Task<IReadOnlyList<RolResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id",
                "Nombre",
                "Descripcion",
                "Activo",
                "FechaCreacion"
            FROM roles
            ORDER BY "Nombre";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var roles = new List<RolResponse>();

        while (
            await reader.ReadAsync(
                cancellationToken))
        {
            roles.Add(
                new RolResponse(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2)
                        ? null
                        : reader.GetString(2),
                    reader.GetBoolean(3),
                    reader.GetFieldValue<DateTime>(4)
                )
            );
        }

        return roles;
    }


    public async Task<RolDetalleResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        const string rolSql = """
            SELECT
                "Id",
                "Nombre",
                "Descripcion",
                "Activo",
                "FechaCreacion"
            FROM roles
            WHERE "Id" = @Id;
            """;

        await using var command =
            dataSource.CreateCommand(rolSql);

        command.Parameters.AddWithValue(
            "Id",
            id);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
                cancellationToken))
        {
            return null;
        }

        var rol = new
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Descripcion = reader.IsDBNull(2)
                ? null
                : reader.GetString(2),
            Activo = reader.GetBoolean(3),
            FechaCreacion =
                reader.GetFieldValue<DateTime>(4)
        };

        await reader.CloseAsync();

        var permisos =
            await GetPermisosByRolAsync(
                id,
                cancellationToken);

        return new RolDetalleResponse(
            rol.Id,
            rol.Nombre,
            rol.Descripcion,
            rol.Activo,
            rol.FechaCreacion,
            permisos
        );
    }


    private async Task<IReadOnlyList<PermisoResponse>>
        GetPermisosByRolAsync(
            int rolId,
            CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p."Id",
                p."Nombre",
                p."Modulo",
                p."Descripcion",
                p."Activo"
            FROM permisos p
            INNER JOIN rol_permisos rp
                ON rp."PermisoId" = p."Id"
            WHERE rp."RolId" = @RolId
            ORDER BY p."Modulo", p."Nombre";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "RolId",
            rolId);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var permisos =
            new List<PermisoResponse>();

        while (
            await reader.ReadAsync(
                cancellationToken))
        {
            permisos.Add(
                new PermisoResponse(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3)
                        ? null
                        : reader.GetString(3),
                    reader.GetBoolean(4)
                )
            );
        }

        return permisos;
    }


    public async Task<RolResponse> CreateAsync(
        CrearRolRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO roles
            (
                "Nombre",
                "Descripcion",
                "Activo"
            )
            VALUES
            (
                @Nombre,
                @Descripcion,
                TRUE
            )
            RETURNING
                "Id",
                "Nombre",
                "Descripcion",
                "Activo",
                "FechaCreacion";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "Nombre",
            request.Nombre.Trim());

        command.Parameters.AddWithValue(
            "Descripcion",
            (object?)request.Descripcion?.Trim()
                ?? DBNull.Value);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        await reader.ReadAsync(
            cancellationToken);

        return new RolResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2)
                ? null
                : reader.GetString(2),
            reader.GetBoolean(3),
            reader.GetFieldValue<DateTime>(4)
        );
    }


    public async Task<RolResponse?> UpdateAsync(
        int id,
        ActualizarRolRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE roles
            SET
                "Nombre" = @Nombre,
                "Descripcion" = @Descripcion
            WHERE "Id" = @Id
            RETURNING
                "Id",
                "Nombre",
                "Descripcion",
                "Activo",
                "FechaCreacion";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "Id",
            id);

        command.Parameters.AddWithValue(
            "Nombre",
            request.Nombre.Trim());

        command.Parameters.AddWithValue(
            "Descripcion",
            (object?)request.Descripcion?.Trim()
                ?? DBNull.Value);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
                cancellationToken))
        {
            return null;
        }

        return new RolResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2)
                ? null
                : reader.GetString(2),
            reader.GetBoolean(3),
            reader.GetFieldValue<DateTime>(4)
        );
    }


    public async Task<bool> SetActiveAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE roles
            SET
                "Activo" = @Activo
            WHERE "Id" = @Id;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "Id",
            id);

        command.Parameters.AddWithValue(
            "Activo",
            activo);

        var affected =
            await command.ExecuteNonQueryAsync(
                cancellationToken);

        return affected > 0;
    }


    // =========================================================
    // ELIMINAR ROL
    // =========================================================

    public async Task<EliminarRolResultado> DeleteAsync(
        int id,
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
            // =================================================
            // 1. OBTENER EL ROL Y BLOQUEARLO
            // =================================================

            const string roleSql = """
                SELECT
                    "Nombre",
                    "Activo"
                FROM roles
                WHERE "Id" = @Id
                FOR UPDATE;
                """;

            await using var roleCommand =
                new NpgsqlCommand(
                    roleSql,
                    connection,
                    transaction);

            roleCommand.Parameters.AddWithValue(
                "Id",
                id);

            await using var roleReader =
                await roleCommand.ExecuteReaderAsync(
                    cancellationToken);

            if (!await roleReader.ReadAsync(
                    cancellationToken))
            {
                await roleReader.CloseAsync();

                await transaction.RollbackAsync(
                    cancellationToken);

                return EliminarRolResultado.NoEncontrado;
            }

            var nombreRol =
                roleReader.GetString(0);

            var activo =
                roleReader.GetBoolean(1);

            await roleReader.CloseAsync();


            // =================================================
            // 2. PROTEGER ADMINISTRADOR
            // =================================================

            if (
                string.Equals(
                    nombreRol,
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase))
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return EliminarRolResultado
                    .AdministradorProtegido;
            }


            // =================================================
            // 3. VALIDAR USUARIOS ASIGNADOS
            // =================================================

            const string usuariosSql = """
                SELECT COUNT(*)
                FROM usuario_roles
                WHERE "RolId" = @Id;
                """;

            await using var usuariosCommand =
                new NpgsqlCommand(
                    usuariosSql,
                    connection,
                    transaction);

            usuariosCommand.Parameters.AddWithValue(
                "Id",
                id);

            var usuariosAsignados =
                Convert.ToInt32(
                    await usuariosCommand.ExecuteScalarAsync(
                        cancellationToken)
                );

            if (usuariosAsignados > 0)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return EliminarRolResultado
                    .TieneUsuarios;
            }


            // =================================================
            // 4. ELIMINAR RELACIONES CON PERMISOS
            // =================================================

            const string permisosSql = """
                DELETE FROM rol_permisos
                WHERE "RolId" = @Id;
                """;

            await using var permisosCommand =
                new NpgsqlCommand(
                    permisosSql,
                    connection,
                    transaction);

            permisosCommand.Parameters.AddWithValue(
                "Id",
                id);

            await permisosCommand.ExecuteNonQueryAsync(
                cancellationToken);


            // =================================================
            // 5. ELIMINAR ROL
            // =================================================

            const string deleteSql = """
                DELETE FROM roles
                WHERE "Id" = @Id;
                """;

            await using var deleteCommand =
                new NpgsqlCommand(
                    deleteSql,
                    connection,
                    transaction);

            deleteCommand.Parameters.AddWithValue(
                "Id",
                id);

            var affected =
                await deleteCommand.ExecuteNonQueryAsync(
                    cancellationToken);

            if (affected == 0)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return EliminarRolResultado.NoEncontrado;
            }


            // =================================================
            // 6. CONFIRMAR TRANSACCIÓN
            // =================================================

            await transaction.CommitAsync(
                cancellationToken);

            return EliminarRolResultado.Eliminado;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }


    // =========================================================
    // PERMISOS
    // =========================================================

    public async Task<IReadOnlyList<PermisoResponse>>
        GetPermisosAsync(
            CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id",
                "Nombre",
                "Modulo",
                "Descripcion",
                "Activo"
            FROM permisos
            WHERE "Activo" = TRUE
            ORDER BY "Modulo", "Nombre";
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var permisos =
            new List<PermisoResponse>();

        while (
            await reader.ReadAsync(
                cancellationToken))
        {
            permisos.Add(
                new PermisoResponse(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3)
                        ? null
                        : reader.GetString(3),
                    reader.GetBoolean(4)
                )
            );
        }

        return permisos;
    }


    public async Task<bool> SetPermisosAsync(
        int rolId,
        IReadOnlyList<int> permisoIds,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        const string roleExistsSql = """
            SELECT EXISTS (
                SELECT 1
                FROM roles
                WHERE "Id" = @RolId
            );
            """;

        await using var roleCommand =
            new NpgsqlCommand(
                roleExistsSql,
                connection,
                transaction);

        roleCommand.Parameters.AddWithValue(
            "RolId",
            rolId);

        var roleExists =
            (bool)(await roleCommand
                .ExecuteScalarAsync(
                    cancellationToken)
                ?? false);

        if (!roleExists)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return false;
        }

        const string deleteSql = """
            DELETE FROM rol_permisos
            WHERE "RolId" = @RolId;
            """;

        await using var deleteCommand =
            new NpgsqlCommand(
                deleteSql,
                connection,
                transaction);

        deleteCommand.Parameters.AddWithValue(
            "RolId",
            rolId);

        await deleteCommand.ExecuteNonQueryAsync(
            cancellationToken);

        const string insertSql = """
            INSERT INTO rol_permisos
            (
                "RolId",
                "PermisoId"
            )
            SELECT
                @RolId,
                p."Id"
            FROM permisos p
            WHERE p."Id" = ANY(@PermisoIds)
              AND p."Activo" = TRUE
            ON CONFLICT
            (
                "RolId",
                "PermisoId"
            )
            DO NOTHING;
            """;

        await using var insertCommand =
            new NpgsqlCommand(
                insertSql,
                connection,
                transaction);

        insertCommand.Parameters.AddWithValue(
            "RolId",
            rolId);

        insertCommand.Parameters.AddWithValue(
            "PermisoIds",
            permisoIds.ToArray());

        await insertCommand.ExecuteNonQueryAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return true;
    }
}