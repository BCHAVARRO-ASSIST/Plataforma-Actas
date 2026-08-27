using Npgsql;
using NpgsqlTypes;

namespace PlataformaActas.Api.Features.Actas;

public sealed class ActaService(NpgsqlDataSource dataSource)
{
    // =====================================================
    // OBTENER TODAS LAS ACTAS
    // ADMINISTRADOR
    // =====================================================

    public async Task<IReadOnlyList<ActaResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id,
                a.numero_acta,
                a.equipment_id,
                e.inventory_key,
                e.nombre_equipo,
                a.usuario_id,
                a.destinatario_usuario_id,
                a.mensajero_usuario_id,
                a.acta_entregada,
                a.estado,
                a.tipo_acta,
                a.acta_origen_id,
                a.ruta_documento,
                a.fecha_firma,
                a.fecha_creacion,
                a.fecha_anulacion,
                a.fecha_finalizacion,
                a.device_id,
                a.form_data_json::text,
                a.hash_sha256,
                a.motivo_anulacion
            FROM actas_v2 AS a
            LEFT JOIN equipment AS e
                ON e.id = a.equipment_id
            ORDER BY a.fecha_creacion DESC;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var actas =
            new List<ActaResponse>();

        while (
            await reader.ReadAsync(
                cancellationToken))
        {
            actas.Add(
                MapActa(reader));
        }

        return actas;
    }


    // =====================================================
    // OBTENER ACTAS DEL MENSAJERO
    // =====================================================

    public async Task<IReadOnlyList<ActaResponse>> GetByMensajeroAsync(
        int usuarioId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id,
                a.numero_acta,
                a.equipment_id,
                e.inventory_key,
                e.nombre_equipo,
                a.usuario_id,
                a.destinatario_usuario_id,
                a.mensajero_usuario_id,
                a.acta_entregada,
                a.estado,
                a.tipo_acta,
                a.acta_origen_id,
                a.ruta_documento,
                a.fecha_firma,
                a.fecha_creacion,
                a.fecha_anulacion,
                a.fecha_finalizacion,
                a.device_id,
                a.form_data_json::text,
                a.hash_sha256,
                a.motivo_anulacion
            FROM actas_v2 AS a
            LEFT JOIN equipment AS e
                ON e.id = a.equipment_id
            WHERE a.mensajero_usuario_id = @usuario_id
            ORDER BY a.fecha_creacion DESC;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.Add(
            "@usuario_id",
            NpgsqlDbType.Integer
        ).Value = usuarioId;

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var actas =
            new List<ActaResponse>();

        while (
            await reader.ReadAsync(
                cancellationToken))
        {
            actas.Add(
                MapActa(reader));
        }

        return actas;
    }


    // =====================================================
    // OBTENER DETALLE DE ACTA
    // =====================================================

    public async Task<ActaDetailResponse?> GetDetailByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id,
                a.numero_acta,
                a.estado,
                a.tipo_acta,
                a.acta_origen_id,
                a_orig.numero_acta AS acta_origen_numero,
                a.acta_entregada,
                a.fecha_creacion,
                a.fecha_firma,
                a.fecha_finalizacion,
                a.fecha_anulacion,
                a.usuario_id,
                a.destinatario_usuario_id,
                COALESCE(u_dest."Nombre" || ' ' || u_dest."Apellido", a.destinatario_usuario_id::text) AS destinatario_nombre,
                u_dest."Correo" AS destinatario_correo,
                u_dest."Proceso" AS destinatario_cargo,
                a.mensajero_usuario_id,
                COALESCE(u_mens."Nombre" || ' ' || u_mens."Apellido", a.mensajero_usuario_id::text) AS mensajero_nombre,
                u_mens."Correo" AS mensajero_correo,
                a.device_id,
                a.form_data_json::text,
                a.signature_data_json::text,
                a.hash_sha256,
                a.motivo_anulacion,

                e.id,
                e.inventory_key,
                e.nombre_equipo,
                e.empresa,
                e.asignado_a,
                e.proveedor,
                e.marca,
                e.procesador,
                e.disco_duro,
                e.memoria_ram,
                e.departamento,
                e.estatus_equipo

            FROM actas_v2 AS a

            INNER JOIN equipment AS e
                ON e.id = a.equipment_id

            LEFT JOIN usuarios AS u_dest
                ON u_dest."Id" = a.destinatario_usuario_id

            LEFT JOIN usuarios AS u_mens
                ON u_mens."Id" = a.mensajero_usuario_id

            LEFT JOIN actas_v2 AS a_orig
                ON a_orig.id = a.acta_origen_id

            WHERE a.id = @id

            LIMIT 1;
            """;


        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.Add(
            "@id",
            NpgsqlDbType.Uuid
        ).Value = id;

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (
            !await reader.ReadAsync(
                cancellationToken))
        {
            return null;
        }

        return MapActaDetail(reader);
    }


    // =====================================================
    // FIRMAR ACTA
    // =====================================================

    public async Task<ActaResponse?> FirmarAsync(
        Guid actaId,
        string signatureDataJson,
        CancellationToken cancellationToken)
    {
        // -------------------------------------------------
        // VALIDAR FIRMA
        // -------------------------------------------------

        if (string.IsNullOrWhiteSpace(
            signatureDataJson))
        {
            throw new InvalidOperationException(
                "La firma manuscrita es obligatoria.");
        }


        // -------------------------------------------------
        // VALIDAR QUE EL ACTA EXISTA Y ESTÉ EN BORRADOR
        // -------------------------------------------------

        const string estadoSql = """
            SELECT
                estado,
                COALESCE(numero_acta, ''),
                equipment_id::text,
                usuario_id,
                COALESCE(destinatario_usuario_id, 0),
                COALESCE(mensajero_usuario_id, 0),
                tipo_acta,
                fecha_creacion
            FROM actas_v2
            WHERE id = @id
            LIMIT 1;
            """;

        string estado;
        string numeroActa;
        string equipmentIdStr;
        int usuarioId;
        int destinatarioId;
        int mensajeroId;
        string tipoActa;
        DateTimeOffset fechaCreacion;

        await using (
            var estadoCommand =
                dataSource.CreateCommand(estadoSql))
        {
            estadoCommand.Parameters.Add(
                "@id",
                NpgsqlDbType.Uuid
            ).Value = actaId;

            await using var estadoReader =
                await estadoCommand.ExecuteReaderAsync(
                    cancellationToken);

            if (!await estadoReader.ReadAsync(cancellationToken))
            {
                return null;
            }

            estado = estadoReader.GetString(0);
            numeroActa = estadoReader.GetString(1);
            equipmentIdStr = estadoReader.GetString(2);
            usuarioId = estadoReader.GetInt32(3);
            destinatarioId = estadoReader.GetInt32(4);
            mensajeroId = estadoReader.GetInt32(5);
            tipoActa = estadoReader.GetString(6);
            fechaCreacion = estadoReader.GetFieldValue<DateTimeOffset>(7);

            if (
                string.Equals(
                    estado,
                    "Firmada",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "El acta ya se encuentra firmada.");
            }

            if (
                !string.Equals(
                    estado,
                    "Borrador",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Solo se puede firmar un acta que se encuentre en estado Borrador.");
            }
        }


        // -------------------------------------------------
        // CALCULAR SHA-256 (CANONICAL PAYLOAD)
        // -------------------------------------------------

        var canonicalPayload =
            $"{actaId}|{numeroActa}|{equipmentIdStr}|{usuarioId}|{destinatarioId}|{mensajeroId}|{tipoActa}|{fechaCreacion:O}|{signatureDataJson}";

        var signatureBytes =
            System.Text.Encoding.UTF8.GetBytes(
                canonicalPayload);

        var hashBytes =
            System.Security.Cryptography.SHA256.HashData(
                signatureBytes);

        var hashSha256 =
            Convert.ToHexString(hashBytes)
                .ToLowerInvariant();



        // -------------------------------------------------
        // ACTUALIZAR ACTA
        // -------------------------------------------------

        const string sql = """
            UPDATE actas_v2
            SET
                signature_data_json = @signature_data_json,
                fecha_firma = NOW(),
                hash_sha256 = @hash_sha256,
                estado = 'Firmada'
            WHERE id = @id;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.Add(
            "@id",
            NpgsqlDbType.Uuid
        ).Value = actaId;

        command.Parameters.Add(
            "@signature_data_json",
            NpgsqlDbType.Json
        ).Value = signatureDataJson;

        command.Parameters.Add(
            "@hash_sha256",
            NpgsqlDbType.Varchar
        ).Value = hashSha256;

        await command.ExecuteNonQueryAsync(cancellationToken);

        // -------------------------------------------------
        // ACTUALIZAR ESTADO DEL EQUIPO
        // -------------------------------------------------

        if (Guid.TryParse(equipmentIdStr, out var parsedEquipmentId))
        {
            if (string.Equals(tipoActa, "Entrega", StringComparison.OrdinalIgnoreCase))
            {
                string? destinatarioNombre = null;
                if (destinatarioId > 0)
                {
                    const string userSql = "SELECT CONCAT(\"Nombre\", ' ', \"Apellido\") FROM usuarios WHERE \"Id\" = @user_id;";
                    await using var userCmd = dataSource.CreateCommand(userSql);
                    userCmd.Parameters.AddWithValue("@user_id", destinatarioId);
                    var nameRes = await userCmd.ExecuteScalarAsync(cancellationToken);
                    destinatarioNombre = nameRes?.ToString();
                }

                const string equipUpdateSql = """
                    UPDATE equipment
                    SET
                        asignado_a = @asignado_a,
                        estatus_equipo = 'Asignado',
                        acta_entregada = true
                    WHERE id = @equipment_id;
                    """;
                await using var equipCmd = dataSource.CreateCommand(equipUpdateSql);
                equipCmd.Parameters.AddWithValue("@asignado_a", (object?)destinatarioNombre ?? DBNull.Value);
                equipCmd.Parameters.AddWithValue("@equipment_id", parsedEquipmentId);
                await equipCmd.ExecuteNonQueryAsync(cancellationToken);
            }
            else if (string.Equals(tipoActa, "Devolución", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(tipoActa, "Devolucion", StringComparison.OrdinalIgnoreCase))
            {
                const string equipDevolucionSql = """
                    UPDATE equipment
                    SET
                        asignado_a = NULL,
                        estatus_equipo = 'Activo',
                        acta_entregada = false
                    WHERE id = @equipment_id;
                    """;
                await using var equipCmd = dataSource.CreateCommand(equipDevolucionSql);
                equipCmd.Parameters.AddWithValue("@equipment_id", parsedEquipmentId);
                await equipCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        // -------------------------------------------------
        // DEVOLVER ACTA ACTUALIZADA
        // -------------------------------------------------

        return await GetByIdAsync(actaId, cancellationToken);
    }

    public async Task<ActaResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id,
                a.numero_acta,
                a.equipment_id,
                e.inventory_key,
                e.nombre_equipo,
                a.usuario_id,
                a.destinatario_usuario_id,
                a.mensajero_usuario_id,
                a.acta_entregada,
                a.estado,
                a.tipo_acta,
                a.acta_origen_id,
                a.ruta_documento,
                a.fecha_firma,
                a.fecha_creacion,
                a.fecha_anulacion,
                a.fecha_finalizacion,
                a.device_id,
                a.form_data_json::text,
                a.hash_sha256,
                a.motivo_anulacion
            FROM actas_v2 AS a
            LEFT JOIN equipment AS e
                ON e.id = a.equipment_id
            WHERE a.id = @id
            LIMIT 1;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.Add("@id", NpgsqlDbType.Uuid).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapActa(reader);
    }



    // =====================================================
    // FINALIZAR ACTA
    // =====================================================

    public async Task<ActaResponse?> FinalizarAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string checkSql = "SELECT estado FROM actas_v2 WHERE id = @id;";
        await using (var checkCmd = dataSource.CreateCommand(checkSql))
        {
            checkCmd.Parameters.AddWithValue("@id", id);
            var estadoObj = await checkCmd.ExecuteScalarAsync(cancellationToken);
            if (estadoObj is null)
            {
                return null;
            }

            var estado = estadoObj.ToString();
            if (!string.Equals(estado, "Firmada", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Solo se puede finalizar un acta que se encuentre en estado Firmada.");
            }
        }

        const string sql = """
            UPDATE actas_v2
            SET
                estado = 'Finalizada',
                fecha_finalizacion = NOW()
            WHERE id = @id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }


    // =====================================================
    // ANULAR ACTA
    // =====================================================

    public async Task<ActaResponse?> AnularAsync(
        Guid id,
        string motivo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("El motivo de anulación es obligatorio.", nameof(motivo));
        }

        string estado;
        Guid equipmentId;
        string tipoActa;

        const string checkSql = "SELECT estado, equipment_id, tipo_acta FROM actas_v2 WHERE id = @id;";
        await using (var checkCmd = dataSource.CreateCommand(checkSql))
        {
            checkCmd.Parameters.AddWithValue("@id", id);
            await using var checkReader = await checkCmd.ExecuteReaderAsync(cancellationToken);
            if (!await checkReader.ReadAsync(cancellationToken))
            {
                return null;
            }

            estado = checkReader.GetString(0);
            equipmentId = checkReader.GetGuid(1);
            tipoActa = checkReader.GetString(2);

            if (string.Equals(estado, "Finalizada", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("No se puede anular un acta que ya fue finalizada.");
            }

            if (string.Equals(estado, "Anulada", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("El acta ya se encuentra anulada.");
            }
        }

        const string sql = """
            UPDATE actas_v2
            SET
                estado = 'Anulada',
                fecha_anulacion = NOW(),
                motivo_anulacion = @motivo
            WHERE id = @id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@motivo", motivo.Trim());

        await command.ExecuteNonQueryAsync(cancellationToken);

        // Si se anuló una entrega que ya estaba procesada, restaurar el equipo a activo
        if (string.Equals(tipoActa, "Entrega", StringComparison.OrdinalIgnoreCase))
        {
            const string restoreEquipSql = """
                UPDATE equipment
                SET
                    asignado_a = NULL,
                    estatus_equipo = 'Activo',
                    acta_entregada = false
                WHERE id = @equipment_id;
                """;
            await using var equipCmd = dataSource.CreateCommand(restoreEquipSql);
            equipCmd.Parameters.AddWithValue("@equipment_id", equipmentId);
            await equipCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }




    // =====================================================
    // CREAR ACTA
    // =====================================================

    public async Task<ActaResponse> CreateAsync(
        CreateActaRequest request,
        CancellationToken cancellationToken)
    {
        var tipoActa =
            NormalizeTipoActa(
                request.TipoActa);


        // =================================================
        // VALIDAR TIPO
        // =================================================

        if (
            tipoActa is not "Entrega"
            &&
            tipoActa is not "Devolución")
        {
            throw new InvalidOperationException(
                "El tipo de acta debe ser 'Entrega' o 'Devolución'.");
        }


        // =================================================
        // VALIDAR EQUIPO
        // =================================================

        const string equipmentSql = """
            SELECT
                estatus_equipo
            FROM equipment
            WHERE id = @equipment_id
            LIMIT 1;
            """;

        await using (
            var equipmentCommand =
                dataSource.CreateCommand(
                    equipmentSql))
        {
            equipmentCommand.Parameters.Add(
                "@equipment_id",
                NpgsqlDbType.Uuid
            ).Value = request.EquipmentId;

            var result =
                await equipmentCommand.ExecuteScalarAsync(
                    cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException(
                    "El equipo seleccionado no existe en el inventario.");
            }

            var estatusEquipo =
                result.ToString();

            if (
                tipoActa == "Entrega"
                &&
                !string.Equals(
                    estatusEquipo,
                    "Activo",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "No se puede crear un acta de entrega para un equipo que no está activo.");
            }
        }


        // =================================================
        // VALIDACIONES ESPECÍFICAS DE ENTREGA
        // =================================================

        if (tipoActa == "Entrega")
        {
            if (request.ActaOrigenId is not null)
            {
                throw new InvalidOperationException(
                    "Una acta de entrega no puede tener un acta de origen.");
            }


            const string entregaVigenteSql = """
                SELECT
                    a.numero_acta
                FROM actas_v2 AS a
                WHERE a.equipment_id = @equipment_id
                  AND LOWER(a.tipo_acta) = 'entrega'
                  AND LOWER(a.estado) <> 'anulada'
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM actas_v2 AS d
                      WHERE d.acta_origen_id = a.id
                        AND LOWER(d.tipo_acta) = 'devolución'
                        AND LOWER(d.estado) <> 'anulada'
                  )
                ORDER BY a.fecha_creacion DESC
                LIMIT 1;
                """;

            await using var entregaCommand =
                dataSource.CreateCommand(
                    entregaVigenteSql);

            entregaCommand.Parameters.Add(
                "@equipment_id",
                NpgsqlDbType.Uuid
            ).Value = request.EquipmentId;

            var entregaVigente =
                await entregaCommand.ExecuteScalarAsync(
                    cancellationToken);

            if (entregaVigente is not null)
            {
                throw new InvalidOperationException(
                    $"El equipo ya tiene un acta de entrega vigente: {entregaVigente}.");
            }
        }


        // =================================================
        // VALIDACIONES ESPECÍFICAS DE DEVOLUCIÓN
        // =================================================

        if (tipoActa == "Devolución")
        {
            if (request.ActaOrigenId is null)
            {
                throw new InvalidOperationException(
                    "Una acta de devolución debe estar relacionada con un acta de entrega.");
            }


            const string origenSql = """
                SELECT
                    id,
                    equipment_id,
                    tipo_acta,
                    estado
                FROM actas_v2
                WHERE id = @acta_origen_id
                LIMIT 1;
                """;

            Guid origenEquipmentId;
            string origenTipoActa;
            string origenEstado;

            await using (
                var origenCommand =
                    dataSource.CreateCommand(
                        origenSql))
            {
                origenCommand.Parameters.Add(
                    "@acta_origen_id",
                    NpgsqlDbType.Uuid
                ).Value =
                    request.ActaOrigenId.Value;

                await using var origenReader =
                    await origenCommand.ExecuteReaderAsync(
                        cancellationToken);

                if (
                    !await origenReader.ReadAsync(
                        cancellationToken))
                {
                    throw new InvalidOperationException(
                        "El acta de origen no existe.");
                }

                origenEquipmentId =
                    origenReader.GetGuid(1);

                origenTipoActa =
                    origenReader.GetString(2);

                origenEstado =
                    origenReader.GetString(3);
            }


            if (
                !string.Equals(
                    origenTipoActa,
                    "Entrega",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "El acta de origen de una devolución debe ser un acta de entrega.");
            }


            if (
                origenEquipmentId
                != request.EquipmentId)
            {
                throw new InvalidOperationException(
                    "El equipo de la devolución no coincide con el equipo del acta de entrega.");
            }


            if (
                !string.Equals(
                    origenEstado,
                    "Firmada",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "El acta de entrega debe estar firmada antes de poder realizar la devolución.");
            }


            const string devolucionExistenteSql = """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM actas_v2
                    WHERE acta_origen_id = @acta_origen_id
                      AND LOWER(tipo_acta) = 'devolución'
                      AND LOWER(estado) <> 'anulada'
                );
                """;

            await using var devolucionCommand =
                dataSource.CreateCommand(
                    devolucionExistenteSql);

            devolucionCommand.Parameters.Add(
                "@acta_origen_id",
                NpgsqlDbType.Uuid
            ).Value =
                request.ActaOrigenId.Value;

            var resultadoDevolucion =
                await devolucionCommand.ExecuteScalarAsync(
                    cancellationToken);

            var devolucionExiste =
                resultadoDevolucion is bool valor &&
                valor;

            if (devolucionExiste)
            {
                throw new InvalidOperationException(
                    "El acta de entrega ya tiene una devolución registrada.");
            }
        }


        // =================================================
        // INSERTAR ACTA
        // =================================================

        const string sql = """
            INSERT INTO actas_v2
            (
                equipment_id,
                usuario_id,
                destinatario_usuario_id,
                mensajero_usuario_id,
                acta_entregada,
                device_id,
                form_data_json,
                tipo_acta,
                acta_origen_id,
                estado,
                numero_acta
            )
            VALUES
            (
                @equipment_id,
                @usuario_id,
                @destinatario_usuario_id,
                @mensajero_usuario_id,
                @acta_entregada,
                @device_id,
                @form_data_json,
                @tipo_acta,
                @acta_origen_id,
                'Borrador',
                @numero_acta
            )
            RETURNING
                id,
                numero_acta,
                equipment_id,
                usuario_id,
                destinatario_usuario_id,
                mensajero_usuario_id,
                acta_entregada,
                estado,
                tipo_acta,
                acta_origen_id,
                ruta_documento,
                fecha_firma,
                fecha_creacion,
                fecha_anulacion,
                fecha_finalizacion,
                device_id,
                form_data_json::text,
                hash_sha256,
                motivo_anulacion;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.Add(
            "@equipment_id",
            NpgsqlDbType.Uuid
        ).Value =
            request.EquipmentId;

        command.Parameters.Add(
            "@usuario_id",
            NpgsqlDbType.Integer
        ).Value =
            request.UsuarioId;

        command.Parameters.Add(
            "@destinatario_usuario_id",
            NpgsqlDbType.Integer
        ).Value =
            request.DestinatarioUsuarioId.HasValue
                ? request.DestinatarioUsuarioId.Value
                : DBNull.Value;

        command.Parameters.Add(
            "@mensajero_usuario_id",
            NpgsqlDbType.Integer
        ).Value =
            request.MensajeroUsuarioId.HasValue
                ? request.MensajeroUsuarioId.Value
                : DBNull.Value;

        command.Parameters.Add(
            "@acta_entregada",
            NpgsqlDbType.Boolean
        ).Value =
            request.ActaEntregada;

        command.Parameters.Add(
            "@device_id",
            NpgsqlDbType.Varchar
        ).Value =
            request.DeviceId is null
                ? DBNull.Value
                : request.DeviceId;

        command.Parameters.Add(
            "@form_data_json",
            NpgsqlDbType.Jsonb
        ).Value =
            string.IsNullOrWhiteSpace(
                request.FormDataJson)
                    ? "{}"
                    : request.FormDataJson;

        command.Parameters.Add(
            "@tipo_acta",
            NpgsqlDbType.Varchar
        ).Value =
            tipoActa;

        command.Parameters.Add(
            "@acta_origen_id",
            NpgsqlDbType.Uuid
        ).Value =
            request.ActaOrigenId.HasValue
                ? request.ActaOrigenId.Value
                : DBNull.Value;

        command.Parameters.Add(
            "@numero_acta",
            NpgsqlDbType.Varchar
        ).Value =
            GenerateActaNumber();

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (
            !await reader.ReadAsync(
                cancellationToken))
        {
            throw new InvalidOperationException(
                "No fue posible crear el acta.");
        }

        return new ActaResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetGuid(2),
            null,
            null,
            reader.GetInt32(3),
            reader.IsDBNull(4)
                ? null
                : reader.GetInt32(4),
            reader.IsDBNull(5)
                ? null
                : reader.GetInt32(5),
            reader.GetBoolean(6),
            reader.GetString(7),
            reader.GetString(8),
            reader.IsDBNull(9)
                ? null
                : reader.GetGuid(9),
            reader.IsDBNull(10)
                ? null
                : reader.GetString(10),
            reader.IsDBNull(11)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(11),
            reader.GetFieldValue<DateTimeOffset>(12),
            reader.IsDBNull(13)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(13),
            reader.IsDBNull(14)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(14),
            reader.IsDBNull(15)
                ? null
                : reader.GetString(15),
            reader.IsDBNull(16)
                ? string.Empty
                : reader.GetString(16),
            reader.IsDBNull(17)
                ? null
                : reader.GetString(17),
            reader.IsDBNull(18)
                ? null
                : reader.GetString(18)
        );
    }


    // =====================================================
    // NORMALIZAR TIPO DE ACTA
    // =====================================================

    private static string NormalizeTipoActa(
        string? tipoActa)
    {
        if (
            string.Equals(
                tipoActa,
                "Entrega",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Entrega";
        }

        if (
            string.Equals(
                tipoActa,
                "Devolución",
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(
                tipoActa,
                "Devolucion",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Devolución";
        }

        return tipoActa?.Trim() ?? string.Empty;
    }


    // =====================================================
    // MAPEAR ACTA
    // =====================================================

    private static ActaResponse MapActa(
        NpgsqlDataReader reader)
    {
        return new ActaResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetGuid(2),
            reader.IsDBNull(3)
                ? null
                : reader.GetString(3),
            reader.IsDBNull(4)
                ? null
                : reader.GetString(4),
            reader.GetInt32(5),
            reader.IsDBNull(6)
                ? null
                : reader.GetInt32(6),
            reader.IsDBNull(7)
                ? null
                : reader.GetInt32(7),
            reader.GetBoolean(8),
            reader.GetString(9),
            reader.GetString(10),
            reader.IsDBNull(11)
                ? null
                : reader.GetGuid(11),
            reader.IsDBNull(12)
                ? null
                : reader.GetString(12),
            reader.IsDBNull(13)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(13),
            reader.GetFieldValue<DateTimeOffset>(14),
            reader.IsDBNull(15)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(15),
            reader.IsDBNull(16)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(16),
            reader.IsDBNull(17)
                ? null
                : reader.GetString(17),
            reader.IsDBNull(18)
                ? string.Empty
                : reader.GetString(18),
            reader.IsDBNull(19)
                ? null
                : reader.GetString(19),
            reader.IsDBNull(20)
                ? null
                : reader.GetString(20)
        );
    }


    // =====================================================
    // MAPEAR DETALLE
    // =====================================================

    private static ActaDetailResponse MapActaDetail(
        NpgsqlDataReader reader)
    {
        if (reader.IsDBNull(24))
        {
            throw new InvalidOperationException(
                "El acta no tiene información de equipo asociada.");
        }

        var equipo =
            new EquipmentActaResponse(
                reader.GetGuid(24),
                reader.IsDBNull(25) ? string.Empty : reader.GetString(25),
                reader.IsDBNull(26) ? string.Empty : reader.GetString(26),
                reader.IsDBNull(27) ? null : reader.GetString(27),
                reader.IsDBNull(28) ? null : reader.GetString(28),
                reader.IsDBNull(29) ? null : reader.GetString(29),
                reader.IsDBNull(30) ? null : reader.GetString(30),
                reader.IsDBNull(31) ? null : reader.GetString(31),
                reader.IsDBNull(32) ? null : reader.GetString(32),
                reader.IsDBNull(33) ? null : reader.GetString(33),
                reader.IsDBNull(34) ? null : reader.GetString(34),
                reader.IsDBNull(35) ? null : reader.GetString(35)
            );

        return new ActaDetailResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetGuid(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.GetBoolean(6),
            reader.GetFieldValue<DateTimeOffset>(7),
            reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8),
            reader.IsDBNull(9) ? null : reader.GetFieldValue<DateTimeOffset>(9),
            reader.IsDBNull(10) ? null : reader.GetFieldValue<DateTimeOffset>(10),
            reader.GetInt32(11),
            reader.IsDBNull(12) ? null : reader.GetInt32(12),
            reader.IsDBNull(13) ? null : reader.GetString(13),
            reader.IsDBNull(14) ? null : reader.GetString(14),
            reader.IsDBNull(15) ? null : reader.GetString(15),
            reader.IsDBNull(16) ? null : reader.GetInt32(16),
            reader.IsDBNull(17) ? null : reader.GetString(17),
            reader.IsDBNull(18) ? null : reader.GetString(18),
            reader.IsDBNull(19) ? null : reader.GetString(19),
            reader.IsDBNull(20) ? string.Empty : reader.GetString(20),
            reader.IsDBNull(21) ? null : reader.GetString(21),
            reader.IsDBNull(22) ? null : reader.GetString(22),
            reader.IsDBNull(23) ? null : reader.GetString(23),
            equipo
        );
    }



    // =====================================================
    // GENERAR NÚMERO DE ACTA
    // =====================================================

    private static string GenerateActaNumber()
    {
        return
            $"ACT-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1, 999999):D6}";
    }
}