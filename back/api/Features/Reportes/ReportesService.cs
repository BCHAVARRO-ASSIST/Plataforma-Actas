using Npgsql;
using NpgsqlTypes;
using System.Text;

namespace PlataformaActas.Api.Features.Reportes;

public sealed class ReportesService(NpgsqlDataSource dataSource)
{
    // =========================================================
    // REPORTE DE EQUIPOS
    // =========================================================

    public async Task<IReadOnlyList<EquiposReportItem>> GetEquiposReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder("""
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
                estatus_equipo,
                COALESCE(acta_entregada, false),
                tipo_mantenimiento,
                fecha_mantenimiento
            FROM equipment
            WHERE 1=1
            """);

        await using var cmd = dataSource.CreateCommand();

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            sb.Append(" AND LOWER(estatus_equipo) = LOWER(@estado)");
            cmd.Parameters.AddWithValue("@estado", filter.Estado.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filter.Empresa))
        {
            sb.Append(" AND LOWER(empresa) LIKE LOWER(@empresa)");
            cmd.Parameters.AddWithValue("@empresa", $"%{filter.Empresa.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filter.Marca))
        {
            sb.Append(" AND LOWER(marca) LIKE LOWER(@marca)");
            cmd.Parameters.AddWithValue("@marca", $"%{filter.Marca.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filter.Proveedor))
        {
            sb.Append(" AND LOWER(proveedor) LIKE LOWER(@proveedor)");
            cmd.Parameters.AddWithValue("@proveedor", $"%{filter.Proveedor.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sb.Append(" AND (LOWER(inventory_key) LIKE LOWER(@search) OR LOWER(nombre_equipo) LIKE LOWER(@search) OR LOWER(COALESCE(asignado_a, '')) LIKE LOWER(@search))");
            cmd.Parameters.AddWithValue("@search", $"%{filter.Search.Trim()}%");
        }

        sb.Append(" ORDER BY inventory_key ASC;");
        cmd.CommandText = sb.ToString();

        var list = new List<EquiposReportItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new EquiposReportItem(
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
                reader.IsDBNull(11) ? null : reader.GetString(11),
                reader.GetBoolean(12),
                reader.IsDBNull(13) ? null : reader.GetString(13),
                reader.IsDBNull(14) ? null : reader.GetFieldValue<DateOnly>(14)
            ));
        }

        return list;
    }


    // =========================================================
    // REPORTE DE ACTAS
    // =========================================================

    public async Task<IReadOnlyList<ActasReportItem>> GetActasReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder("""
            SELECT
                a.id,
                a.numero_acta,
                a.tipo_acta,
                a.estado,
                a.equipment_id,
                e.inventory_key,
                e.nombre_equipo,
                a.usuario_id,
                CONCAT(u1."Nombre", ' ', u1."Apellido") AS creador_nombre,
                a.destinatario_usuario_id,
                CONCAT(u2."Nombre", ' ', u2."Apellido") AS destinatario_nombre,
                a.mensajero_usuario_id,
                CONCAT(u3."Nombre", ' ', u3."Apellido") AS mensajero_nombre,
                a.acta_entregada,
                a.fecha_creacion,
                a.fecha_firma,
                a.fecha_finalizacion,
                a.fecha_anulacion,
                a.acta_origen_id
            FROM actas_v2 a
            LEFT JOIN equipment e ON e.id = a.equipment_id
            LEFT JOIN usuarios u1 ON u1."Id" = a.usuario_id
            LEFT JOIN usuarios u2 ON u2."Id" = a.destinatario_usuario_id
            LEFT JOIN usuarios u3 ON u3."Id" = a.mensajero_usuario_id
            WHERE 1=1
            """);

        await using var cmd = dataSource.CreateCommand();

        if (filter.FechaInicio.HasValue)
        {
            sb.Append(" AND a.fecha_creacion >= @fecha_inicio");
            cmd.Parameters.Add("@fecha_inicio", NpgsqlDbType.TimestampTz).Value = filter.FechaInicio.Value;
        }

        if (filter.FechaFin.HasValue)
        {
            sb.Append(" AND a.fecha_creacion <= @fecha_fin");
            cmd.Parameters.Add("@fecha_fin", NpgsqlDbType.TimestampTz).Value = filter.FechaFin.Value;
        }

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            sb.Append(" AND LOWER(a.estado) = LOWER(@estado)");
            cmd.Parameters.AddWithValue("@estado", filter.Estado.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filter.TipoActa))
        {
            sb.Append(" AND (LOWER(a.tipo_acta) = LOWER(@tipo) OR (LOWER(@tipo) = 'devolucion' AND LOWER(a.tipo_acta) = 'devolución'))");
            cmd.Parameters.AddWithValue("@tipo", filter.TipoActa.Trim());
        }

        if (filter.EquipmentId.HasValue)
        {
            sb.Append(" AND a.equipment_id = @equip_id");
            cmd.Parameters.AddWithValue("@equip_id", filter.EquipmentId.Value);
        }

        if (filter.MensajeroId.HasValue)
        {
            sb.Append(" AND a.mensajero_usuario_id = @mensajero_id");
            cmd.Parameters.AddWithValue("@mensajero_id", filter.MensajeroId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sb.Append(" AND (LOWER(a.numero_acta) LIKE LOWER(@search) OR LOWER(COALESCE(e.inventory_key, '')) LIKE LOWER(@search) OR LOWER(COALESCE(e.nombre_equipo, '')) LIKE LOWER(@search))");
            cmd.Parameters.AddWithValue("@search", $"%{filter.Search.Trim()}%");
        }

        sb.Append(" ORDER BY a.fecha_creacion DESC;");
        cmd.CommandText = sb.ToString();

        var list = new List<ActasReportItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new ActasReportItem(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetGuid(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetInt32(9),
                reader.IsDBNull(10) ? null : reader.GetString(10),
                reader.IsDBNull(11) ? null : reader.GetInt32(11),
                reader.IsDBNull(12) ? null : reader.GetString(12),
                reader.GetBoolean(13),
                reader.GetFieldValue<DateTimeOffset>(14),
                reader.IsDBNull(15) ? null : reader.GetFieldValue<DateTimeOffset>(15),
                reader.IsDBNull(16) ? null : reader.GetFieldValue<DateTimeOffset>(16),
                reader.IsDBNull(17) ? null : reader.GetFieldValue<DateTimeOffset>(17),
                reader.IsDBNull(18) ? null : reader.GetGuid(18)
            ));
        }

        return list;
    }


    // =========================================================
    // REPORTE DE ENTREGAS
    // =========================================================

    public async Task<IReadOnlyList<EntregasReportItem>> GetEntregasReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder("""
            SELECT
                a.id,
                a.numero_acta,
                COALESCE(e.inventory_key, 'N/A') AS equip_key,
                COALESCE(e.nombre_equipo, 'N/A') AS equip_nombre,
                CONCAT(u1."Nombre", ' ', u1."Apellido") AS mensajero_nombre,
                CONCAT(u2."Nombre", ' ', u2."Apellido") AS destinatario_nombre,
                a.estado,
                (a.fecha_firma IS NOT NULL OR a.estado = 'Firmada' OR a.estado = 'Finalizada') AS firmado,
                a.fecha_creacion,
                a.fecha_firma
            FROM actas_v2 a
            LEFT JOIN equipment e ON e.id = a.equipment_id
            LEFT JOIN usuarios u1 ON u1."Id" = a.mensajero_usuario_id
            LEFT JOIN usuarios u2 ON u2."Id" = a.destinatario_usuario_id
            WHERE LOWER(a.tipo_acta) = 'entrega'
            """);

        await using var cmd = dataSource.CreateCommand();

        if (filter.FechaInicio.HasValue)
        {
            sb.Append(" AND a.fecha_creacion >= @fecha_inicio");
            cmd.Parameters.Add("@fecha_inicio", NpgsqlDbType.TimestampTz).Value = filter.FechaInicio.Value;
        }

        if (filter.FechaFin.HasValue)
        {
            sb.Append(" AND a.fecha_creacion <= @fecha_fin");
            cmd.Parameters.Add("@fecha_fin", NpgsqlDbType.TimestampTz).Value = filter.FechaFin.Value;
        }

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            sb.Append(" AND LOWER(a.estado) = LOWER(@estado)");
            cmd.Parameters.AddWithValue("@estado", filter.Estado.Trim());
        }

        if (filter.MensajeroId.HasValue)
        {
            sb.Append(" AND a.mensajero_usuario_id = @mensajero_id");
            cmd.Parameters.AddWithValue("@mensajero_id", filter.MensajeroId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sb.Append(" AND (LOWER(a.numero_acta) LIKE LOWER(@search) OR LOWER(COALESCE(e.inventory_key, '')) LIKE LOWER(@search))");
            cmd.Parameters.AddWithValue("@search", $"%{filter.Search.Trim()}%");
        }

        sb.Append(" ORDER BY a.fecha_creacion DESC;");
        cmd.CommandText = sb.ToString();

        var list = new List<EntregasReportItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new EntregasReportItem(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.GetString(6),
                reader.GetBoolean(7),
                reader.GetFieldValue<DateTimeOffset>(8),
                reader.IsDBNull(9) ? null : reader.GetFieldValue<DateTimeOffset>(9)
            ));
        }

        return list;
    }


    // =========================================================
    // REPORTE DE DEVOLUCIONES
    // =========================================================

    public async Task<IReadOnlyList<DevolucionesReportItem>> GetDevolucionesReportAsync(
        ReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder("""
            SELECT
                a.id,
                a.numero_acta,
                a.acta_origen_id,
                ao.numero_acta AS acta_origen_numero,
                COALESCE(e.inventory_key, 'N/A') AS equip_key,
                COALESCE(e.nombre_equipo, 'N/A') AS equip_nombre,
                CONCAT(u."Nombre", ' ', u."Apellido") AS responsable_entrega,
                a.estado,
                a.fecha_creacion,
                a.fecha_firma
            FROM actas_v2 a
            LEFT JOIN actas_v2 ao ON ao.id = a.acta_origen_id
            LEFT JOIN equipment e ON e.id = a.equipment_id
            LEFT JOIN usuarios u ON u."Id" = a.destinatario_usuario_id
            WHERE LOWER(a.tipo_acta) = 'devolución' OR LOWER(a.tipo_acta) = 'devolucion'
            """);

        await using var cmd = dataSource.CreateCommand();

        if (filter.FechaInicio.HasValue)
        {
            sb.Append(" AND a.fecha_creacion >= @fecha_inicio");
            cmd.Parameters.Add("@fecha_inicio", NpgsqlDbType.TimestampTz).Value = filter.FechaInicio.Value;
        }

        if (filter.FechaFin.HasValue)
        {
            sb.Append(" AND a.fecha_creacion <= @fecha_fin");
            cmd.Parameters.Add("@fecha_fin", NpgsqlDbType.TimestampTz).Value = filter.FechaFin.Value;
        }

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            sb.Append(" AND LOWER(a.estado) = LOWER(@estado)");
            cmd.Parameters.AddWithValue("@estado", filter.Estado.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sb.Append(" AND (LOWER(a.numero_acta) LIKE LOWER(@search) OR LOWER(COALESCE(e.inventory_key, '')) LIKE LOWER(@search))");
            cmd.Parameters.AddWithValue("@search", $"%{filter.Search.Trim()}%");
        }

        sb.Append(" ORDER BY a.fecha_creacion DESC;");
        cmd.CommandText = sb.ToString();

        var list = new List<DevolucionesReportItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new DevolucionesReportItem(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetGuid(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.GetString(7),
                reader.GetFieldValue<DateTimeOffset>(8),
                reader.IsDBNull(9) ? null : reader.GetFieldValue<DateTimeOffset>(9)
            ));
        }

        return list;
    }
}
