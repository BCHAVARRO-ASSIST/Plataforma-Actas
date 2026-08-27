using Npgsql;

namespace PlataformaActas.Api.Features.Equipment;

public sealed class EquipmentService(NpgsqlDataSource dataSource)
{
    // =====================================================
    // LISTAR EQUIPOS
    // =====================================================

    public async Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id,
                inventory_key,
                nombre_equipo,
                empresa,
                asignado_a,
                proveedor,
                marca,
                procesador,
                disco_duro,
                memoria_ram,
                departamento,
                estatus_equipo
            FROM equipment
            ORDER BY nombre_equipo;
            """;

        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var equipment = new List<EquipmentResponse>();

        while (await reader.ReadAsync(cancellationToken))
        {
            equipment.Add(MapEquipment(reader));
        }

        return equipment;
    }


    // =====================================================
    // REGISTRAR EQUIPO
    // =====================================================

    public async Task<EquipmentResponse> CreateAsync(
        CreateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryKey = request.InventoryKey.Trim();
        var nombreEquipo = request.NombreEquipo.Trim();

        if (string.IsNullOrWhiteSpace(inventoryKey))
        {
            throw new ArgumentException("El código de inventario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombreEquipo))
        {
            throw new ArgumentException("El nombre del equipo es obligatorio.");
        }

        // Validar inventario duplicado
        const string duplicateSql = """
            SELECT EXISTS (
                SELECT 1
                FROM equipment
                WHERE LOWER(inventory_key) = LOWER(@inventory_key)
            );
            """;

        await using (var duplicateCommand = dataSource.CreateCommand(duplicateSql))
        {
            duplicateCommand.Parameters.AddWithValue("@inventory_key", inventoryKey);

            var exists = (bool)(await duplicateCommand.ExecuteScalarAsync(cancellationToken) ?? false);

            if (exists)
            {
                throw new InvalidOperationException("Ya existe un equipo con ese código de inventario.");
            }
        }

        const string sql = """
            INSERT INTO equipment
            (
                id,
                inventory_key,
                nombre_equipo,
                empresa,
                asignado_a,
                proveedor,
                marca,
                procesador,
                disco_duro,
                memoria_ram,
                departamento,
                estatus_equipo
            )
            VALUES
            (
                @id,
                @inventory_key,
                @nombre_equipo,
                @empresa,
                @asignado_a,
                @proveedor,
                @marca,
                @procesador,
                @disco_duro,
                @memoria_ram,
                @departamento,
                @estatus_equipo
            )
            RETURNING
                id,
                inventory_key,
                nombre_equipo,
                empresa,
                asignado_a,
                proveedor,
                marca,
                procesador,
                disco_duro,
                memoria_ram,
                departamento,
                estatus_equipo;
            """;

        var id = Guid.NewGuid();

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@inventory_key", inventoryKey);
        command.Parameters.AddWithValue("@nombre_equipo", nombreEquipo);
        command.Parameters.AddWithValue("@empresa", (object?)request.Empresa?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@asignado_a", (object?)request.AsignadoA?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@proveedor", (object?)request.Proveedor?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@marca", (object?)request.Marca?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@procesador", (object?)request.Procesador?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@disco_duro", (object?)request.DiscoDuro?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@memoria_ram", (object?)request.MemoriaRam?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@departamento", (object?)request.Departamento?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@estatus_equipo", (object?)request.EstatusEquipo?.Trim() ?? "Activo");

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("No fue posible registrar el equipo.");
        }

        return MapEquipment(reader);
    }


    // =====================================================
    // EDITAR EQUIPO
    // =====================================================

    public async Task<EquipmentResponse?> UpdateAsync(
        Guid id,
        UpdateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryKey = request.InventoryKey.Trim();
        var nombreEquipo = request.NombreEquipo.Trim();

        if (string.IsNullOrWhiteSpace(inventoryKey))
        {
            throw new ArgumentException("El código de inventario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombreEquipo))
        {
            throw new ArgumentException("El nombre del equipo es obligatorio.");
        }

        // Validar inventario duplicado en otro equipo
        const string duplicateSql = """
            SELECT EXISTS (
                SELECT 1
                FROM equipment
                WHERE LOWER(inventory_key) = LOWER(@inventory_key)
                  AND id <> @id
            );
            """;

        await using (var duplicateCommand = dataSource.CreateCommand(duplicateSql))
        {
            duplicateCommand.Parameters.AddWithValue("@inventory_key", inventoryKey);
            duplicateCommand.Parameters.AddWithValue("@id", id);

            var exists = (bool)(await duplicateCommand.ExecuteScalarAsync(cancellationToken) ?? false);

            if (exists)
            {
                throw new InvalidOperationException("Ya existe otro equipo con ese código de inventario.");
            }
        }

        const string sql = """
            UPDATE equipment
            SET
                inventory_key = @inventory_key,
                nombre_equipo = @nombre_equipo,
                empresa = @empresa,
                asignado_a = @asignado_a,
                proveedor = @proveedor,
                marca = @marca,
                procesador = @procesador,
                disco_duro = @disco_duro,
                memoria_ram = @memoria_ram,
                departamento = @departamento,
                estatus_equipo = @estatus_equipo
            WHERE id = @id
            RETURNING
                id,
                inventory_key,
                nombre_equipo,
                empresa,
                asignado_a,
                proveedor,
                marca,
                procesador,
                disco_duro,
                memoria_ram,
                departamento,
                estatus_equipo;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@inventory_key", inventoryKey);
        command.Parameters.AddWithValue("@nombre_equipo", nombreEquipo);
        command.Parameters.AddWithValue("@empresa", (object?)request.Empresa?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@asignado_a", (object?)request.AsignadoA?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@proveedor", (object?)request.Proveedor?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@marca", (object?)request.Marca?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@procesador", (object?)request.Procesador?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@disco_duro", (object?)request.DiscoDuro?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@memoria_ram", (object?)request.MemoriaRam?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@departamento", (object?)request.Departamento?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@estatus_equipo", (object?)request.EstatusEquipo?.Trim() ?? "Activo");

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapEquipment(reader);
    }


    // =====================================================
    // DESACTIVAR EQUIPO
    // =====================================================

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE equipment
            SET estatus_equipo = 'Inactivo'
            WHERE id = @id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("@id", id);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }


    // =====================================================
    // ACTIVAR EQUIPO
    // =====================================================

    public async Task<bool> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE equipment
            SET estatus_equipo = 'Activo'
            WHERE id = @id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("@id", id);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }


    // =====================================================
    // ELIMINAR EQUIPO DEFINITIVAMENTE (PARA INACTIVOS)
    // =====================================================

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        // 1. Verificar si está asociado a actas no anuladas
        const string checkActasSql = """
            SELECT COUNT(*)
            FROM actas_v2
            WHERE equipment_id = @id AND estado != 'Anulada';
            """;

        await using (var checkCmd = dataSource.CreateCommand(checkActasSql))
        {
            checkCmd.Parameters.AddWithValue("@id", id);
            var actasCount = (long)(await checkCmd.ExecuteScalarAsync(cancellationToken) ?? 0);
            if (actasCount > 0)
            {
                throw new InvalidOperationException(
                    "No es posible eliminar el equipo porque cuenta con actas activas registradas.");
            }
        }

        // 2. Eliminar de base de datos
        const string deleteSql = "DELETE FROM equipment WHERE id = @id;";
        await using var deleteCmd = dataSource.CreateCommand(deleteSql);
        deleteCmd.Parameters.AddWithValue("@id", id);

        var affected = await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }


    // =====================================================
    // GESTIÓN DE MANTENIMIENTO
    // =====================================================

    public async Task<IReadOnlyList<EquipmentMaintenanceItem>> GetMaintenanceListAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id,
                inventory_key,
                nombre_equipo,
                marca,
                asignado_a,
                departamento,
                estatus_equipo,
                tipo_mantenimiento,
                fecha_mantenimiento,
                COALESCE(mantenimiento_realizado, false)
            FROM equipment
            ORDER BY fecha_mantenimiento DESC NULLS LAST, nombre_equipo ASC;
            """;

        var list = new List<EquipmentMaintenanceItem>();
        await using var cmd = dataSource.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new EquipmentMaintenanceItem(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetFieldValue<DateOnly>(8),
                reader.GetBoolean(9)
            ));
        }

        return list;
    }

    public async Task<bool> UpdateMaintenanceAsync(
        Guid id,
        UpdateMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE equipment
            SET
                tipo_mantenimiento = @tipo,
                fecha_mantenimiento = @fecha,
                mantenimiento_realizado = @realizado
            WHERE id = @id;
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@tipo", (object?)request.TipoMantenimiento?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@fecha", (object?)request.FechaMantenimiento ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@realizado", request.MantenimientoRealizado);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }


    // =====================================================
    // MAPEAR EQUIPO
    // =====================================================

    private static EquipmentResponse MapEquipment(
        NpgsqlDataReader reader)
    {
        return new EquipmentResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            reader.IsDBNull(10) ? null : reader.GetString(10),
            reader.IsDBNull(11) ? null : reader.GetString(11)
        );
    }
}