using Npgsql;
using NpgsqlTypes;

namespace PlataformaActas.Api.Features.Dashboard;

public sealed class DashboardService(NpgsqlDataSource dataSource)
{
    // =========================================================
    // OBTENER INDICADORES KPI
    // =========================================================

    public async Task<DashboardKpiResponse> GetKpisAsync(
        CancellationToken cancellationToken)
    {
        int totalEquipos = 0, equiposActivos = 0, equiposInactivos = 0, equiposAsignados = 0, equiposMantenimiento = 0;
        int totalActas = 0, actasBorrador = 0, actasFirmadas = 0, actasFinalizadas = 0, actasAnuladas = 0;
        int entregasPendientes = 0, devolucionesPendientes = 0;

        // 1. Estadísticas de Equipos
        const string equipSql = """
            SELECT
                COUNT(*) AS total,
                COUNT(CASE WHEN LOWER(estatus_equipo) = 'activo' THEN 1 END) AS activos,
                COUNT(CASE WHEN LOWER(estatus_equipo) = 'inactivo' THEN 1 END) AS inactivos,
                COUNT(CASE WHEN LOWER(estatus_equipo) = 'asignado' THEN 1 END) AS asignados,
                COUNT(CASE WHEN LOWER(estatus_equipo) LIKE '%mantenimiento%' OR (mantenimiento_realizado = true AND fecha_mantenimiento IS NULL) THEN 1 END) AS mantenimiento
            FROM equipment;
            """;

        await using (var cmd = dataSource.CreateCommand(equipSql))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                totalEquipos = reader.GetInt32(0);
                equiposActivos = reader.GetInt32(1);
                equiposInactivos = reader.GetInt32(2);
                equiposAsignados = reader.GetInt32(3);
                equiposMantenimiento = reader.GetInt32(4);
            }
        }

        // 2. Estadísticas de Actas
        const string actasSql = """
            SELECT
                COUNT(*) AS total,
                COUNT(CASE WHEN LOWER(estado) = 'borrador' THEN 1 END) AS borrador,
                COUNT(CASE WHEN LOWER(estado) = 'firmada' THEN 1 END) AS firmadas,
                COUNT(CASE WHEN LOWER(estado) = 'finalizada' THEN 1 END) AS finalizadas,
                COUNT(CASE WHEN LOWER(estado) = 'anulada' THEN 1 END) AS anuladas,
                COUNT(CASE WHEN LOWER(estado) = 'borrador' AND (LOWER(tipo_acta) = 'entrega' OR tipo_acta IS NULL) THEN 1 END) AS entregas_pend,
                COUNT(CASE WHEN LOWER(estado) = 'borrador' AND (LOWER(tipo_acta) = 'devolución' OR LOWER(tipo_acta) = 'devolucion') THEN 1 END) AS dev_pend
            FROM actas_v2;
            """;

        await using (var cmd = dataSource.CreateCommand(actasSql))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                totalActas = reader.GetInt32(0);
                actasBorrador = reader.GetInt32(1);
                actasFirmadas = reader.GetInt32(2);
                actasFinalizadas = reader.GetInt32(3);
                actasAnuladas = reader.GetInt32(4);
                entregasPendientes = reader.GetInt32(5);
                devolucionesPendientes = reader.GetInt32(6);
            }
        }

        return new DashboardKpiResponse(
            totalEquipos,
            equiposActivos,
            equiposInactivos,
            equiposAsignados,
            equiposMantenimiento,
            totalActas,
            actasBorrador,
            actasFirmadas,
            actasFinalizadas,
            actasAnuladas,
            entregasPendientes,
            devolucionesPendientes
        );
    }


    // =========================================================
    // DISTRIBUCIÓN DE ESTADOS DE EQUIPOS
    // =========================================================

    public async Task<IReadOnlyList<StatusCountItem>> GetEquipmentStatusChartAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                COALESCE(NULLIF(TRIM(estatus_equipo), ''), 'Sin estado') AS estado,
                COUNT(*) AS cantidad
            FROM equipment
            GROUP BY 1
            ORDER BY cantidad DESC;
            """;

        var items = new List<(string Estado, int Cantidad)>();
        int total = 0;

        await using (var cmd = dataSource.CreateCommand(sql))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var estado = reader.GetString(0);
                var cantidad = (int)reader.GetInt64(1);
                items.Add((estado, cantidad));
                total += cantidad;
            }
        }

        return items.Select(x =>
        {
            var pct = total > 0 ? Math.Round((double)x.Cantidad / total * 100, 1) : 0;
            var color = x.Estado.ToLower() switch
            {
                "activo" => "#10b981",
                "inactivo" => "#ef4444",
                "asignado" => "#3b82f6",
                "mantenimiento" => "#f59e0b",
                _ => "#6b7280"
            };
            return new StatusCountItem(x.Estado, x.Cantidad, pct, color);
        }).ToList();
    }


    // =========================================================
    // DISTRIBUCIÓN DE ESTADOS DE ACTAS
    // =========================================================

    public async Task<IReadOnlyList<StatusCountItem>> GetActasStatusChartAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                COALESCE(NULLIF(TRIM(estado), ''), 'Sin estado') AS estado,
                COUNT(*) AS cantidad
            FROM actas_v2
            GROUP BY 1
            ORDER BY cantidad DESC;
            """;

        var items = new List<(string Estado, int Cantidad)>();
        int total = 0;

        await using (var cmd = dataSource.CreateCommand(sql))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var estado = reader.GetString(0);
                var cantidad = (int)reader.GetInt64(1);
                items.Add((estado, cantidad));
                total += cantidad;
            }
        }

        return items.Select(x =>
        {
            var pct = total > 0 ? Math.Round((double)x.Cantidad / total * 100, 1) : 0;
            var color = x.Estado.ToLower() switch
            {
                "borrador" => "#f59e0b",
                "firmada" => "#10b981",
                "finalizada" => "#3b82f6",
                "anulada" => "#ef4444",
                _ => "#6b7280"
            };
            return new StatusCountItem(x.Estado, x.Cantidad, pct, color);
        }).ToList();
    }


    // =========================================================
    // ENTREGAS POR MENSAJERO
    // =========================================================

    public async Task<IReadOnlyList<MessengerDeliveryItem>> GetDeliveriesByMessengerAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.mensajero_usuario_id,
                COALESCE(CONCAT(u."Nombre", ' ', u."Apellido"), 'Sin mensajero asignado') AS mensajero_nombre,
                COUNT(*) AS total_asignadas,
                COUNT(CASE WHEN a.estado = 'Firmada' OR a.estado = 'Finalizada' OR a.acta_entregada = true THEN 1 END) AS total_entregadas,
                COUNT(CASE WHEN a.estado = 'Borrador' THEN 1 END) AS total_pendientes
            FROM actas_v2 a
            LEFT JOIN usuarios u ON u."Id" = a.mensajero_usuario_id
            GROUP BY a.mensajero_usuario_id, u."Nombre", u."Apellido"
            ORDER BY total_asignadas DESC;
            """;

        var result = new List<MessengerDeliveryItem>();

        await using var cmd = dataSource.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var mensajeroId = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
            var nombre = reader.GetString(1);
            var asignadas = (int)reader.GetInt64(2);
            var entregadas = (int)reader.GetInt64(3);
            var pendientes = (int)reader.GetInt64(4);

            result.Add(new MessengerDeliveryItem(
                mensajeroId,
                nombre,
                asignadas,
                entregadas,
                pendientes
            ));
        }

        return result;
    }


    // =========================================================
    // LÍNEA TEMPORAL DE ACTIVIDAD
    // =========================================================

    public async Task<IReadOnlyList<TimelinePointItem>> GetTimelineAsync(
        string? range,
        CancellationToken cancellationToken)
    {
        var dias = range?.ToLower() switch
        {
            "7d" => 7,
            "90d" => 90,
            "month" => DateTime.UtcNow.Day,
            _ => 30
        };

        var fechaCorte = DateTimeOffset.UtcNow.AddDays(-dias);

        const string sql = """
            SELECT
                TO_CHAR(DATE_TRUNC('day', fecha_creacion), 'YYYY-MM-DD') AS dia,
                COUNT(*) AS total_creadas,
                COUNT(CASE WHEN estado = 'Firmada' OR estado = 'Finalizada' THEN 1 END) AS total_firmadas,
                COUNT(CASE WHEN LOWER(tipo_acta) = 'entrega' THEN 1 END) AS total_entregas,
                COUNT(CASE WHEN LOWER(tipo_acta) = 'devolución' OR LOWER(tipo_acta) = 'devolucion' THEN 1 END) AS total_devoluciones
            FROM actas_v2
            WHERE fecha_creacion >= @fecha_corte
            GROUP BY DATE_TRUNC('day', fecha_creacion)
            ORDER BY dia ASC;
            """;

        var result = new List<TimelinePointItem>();

        await using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.Add("@fecha_corte", NpgsqlDbType.TimestampTz).Value = fechaCorte;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new TimelinePointItem(
                reader.GetString(0),
                (int)reader.GetInt64(1),
                (int)reader.GetInt64(2),
                (int)reader.GetInt64(3),
                (int)reader.GetInt64(4)
            ));
        }

        return result;
    }


    // =========================================================
    // ESTADÍSTICAS DE MANTENIMIENTO
    // =========================================================

    public async Task<MaintenanceStatsResponse> GetMaintenanceStatsAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                COUNT(*) AS total,
                COUNT(CASE WHEN LOWER(tipo_mantenimiento) LIKE '%preventivo%' THEN 1 END) AS preventivos,
                COUNT(CASE WHEN LOWER(tipo_mantenimiento) LIKE '%correctivo%' THEN 1 END) AS correctivos,
                COUNT(CASE WHEN mantenimiento_realizado = true THEN 1 END) AS concluidos,
                COUNT(CASE WHEN mantenimiento_realizado = false OR (mantenimiento_realizado IS NULL AND tipo_mantenimiento IS NOT NULL) THEN 1 END) AS pendientes
            FROM equipment
            WHERE tipo_mantenimiento IS NOT NULL OR mantenimiento_realizado IS NOT NULL OR fecha_mantenimiento IS NOT NULL;
            """;

        int total = 0, preventivos = 0, correctivos = 0, concluidos = 0, pendientes = 0;

        await using (var cmd = dataSource.CreateCommand(sql))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                total = (int)reader.GetInt64(0);
                preventivos = (int)reader.GetInt64(1);
                correctivos = (int)reader.GetInt64(2);
                concluidos = (int)reader.GetInt64(3);
                pendientes = (int)reader.GetInt64(4);
            }
        }

        // Desglose por tipo
        const string tiposSql = """
            SELECT
                COALESCE(NULLIF(TRIM(tipo_mantenimiento), ''), 'General / Sin especificar') AS tipo,
                COUNT(*) AS cantidad
            FROM equipment
            WHERE tipo_mantenimiento IS NOT NULL OR mantenimiento_realizado = true
            GROUP BY 1
            ORDER BY cantidad DESC;
            """;

        var tipos = new List<StatusCountItem>();
        await using (var cmd = dataSource.CreateCommand(tiposSql))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var tipo = reader.GetString(0);
                var cant = (int)reader.GetInt64(1);
                var pct = total > 0 ? Math.Round((double)cant / total * 100, 1) : 0;
                tipos.Add(new StatusCountItem(tipo, cant, pct, "#6366f1"));
            }
        }

        return new MaintenanceStatsResponse(
            total,
            preventivos,
            correctivos,
            concluidos,
            pendientes,
            tipos
        );
    }


    // =========================================================
    // ACTIVIDAD RECIENTE
    // =========================================================

    public async Task<IReadOnlyList<RecentActivityItem>> GetRecentActivityAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.id::text,
                CASE
                    WHEN a.estado = 'Borrador' THEN 'Acta Creada'
                    WHEN a.estado = 'Firmada' THEN 'Acta Firmada'
                    WHEN a.estado = 'Finalizada' THEN 'Acta Finalizada'
                    WHEN a.estado = 'Anulada' THEN 'Acta Anulada'
                    ELSE 'Actualización de Acta'
                END AS tipo_evento,
                CONCAT('Acta ', a.numero_acta, ' (', a.tipo_acta, ')') AS titulo,
                CONCAT('Equipo: ', COALESCE(e.nombre_equipo, e.inventory_key, 'N/A')) AS descripcion,
                COALESCE(CONCAT(u."Nombre", ' ', u."Apellido"), 'Sistema') AS responsable,
                COALESCE(a.fecha_firma, a.fecha_anulacion, a.fecha_finalizacion, a.fecha_creacion) AS fecha,
                a.estado,
                a.tipo_acta AS badge_tipo
            FROM actas_v2 a
            LEFT JOIN equipment e ON e.id = a.equipment_id
            LEFT JOIN usuarios u ON u."Id" = a.usuario_id
            ORDER BY COALESCE(a.fecha_firma, a.fecha_anulacion, a.fecha_finalizacion, a.fecha_creacion) DESC
            LIMIT @limit;
            """;

        var result = new List<RecentActivityItem>();

        await using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@limit", Math.Clamp(limit, 5, 50));

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new RecentActivityItem(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetFieldValue<DateTimeOffset>(5),
                reader.GetString(6),
                reader.GetString(7)
            ));
        }

        return result;
    }
}
