namespace PlataformaActas.Api.Features.Dashboard;

// =========================================================
// INDICADORES KPI
// =========================================================

public sealed record DashboardKpiResponse(
    int TotalEquipos,
    int EquiposActivos,
    int EquiposInactivos,
    int EquiposAsignados,
    int EquiposMantenimiento,
    int TotalActas,
    int ActasBorrador,
    int ActasFirmadas,
    int ActasFinalizadas,
    int ActasAnuladas,
    int EntregasPendientes,
    int DevolucionesPendientes
);

// =========================================================
// ELEMENTO DE DISTRIBUCIÓN POR ESTADO
// =========================================================

public sealed record StatusCountItem(
    string Estado,
    int Cantidad,
    double Porcentaje,
    string? Color
);

// =========================================================
// ENTREGAS POR MENSAJERO
// =========================================================

public sealed record MessengerDeliveryItem(
    int? MensajeroId,
    string MensajeroNombre,
    int TotalAsignadas,
    int TotalEntregadas,
    int TotalPendientes
);

// =========================================================
// LÍNEA TEMPORAL DE ACTIVIDAD
// =========================================================

public sealed record TimelinePointItem(
    string Fecha,
    int TotalCreadas,
    int TotalFirmadas,
    int TotalEntregas,
    int TotalDevoluciones
);

// =========================================================
// ESTADÍSTICAS DE MANTENIMIENTO
// =========================================================

public sealed record MaintenanceStatsResponse(
    int TotalMantenimientos,
    int Preventivos,
    int Correctivos,
    int Concluidos,
    int Pendientes,
    IReadOnlyList<StatusCountItem> MantenimientosPorTipo
);

// =========================================================
// ACTIVIDAD RECIENTE
// =========================================================

public sealed record RecentActivityItem(
    string Id,
    string TipoEvento,
    string Titulo,
    string Descripcion,
    string? Responsable,
    DateTimeOffset Fecha,
    string Estado,
    string? BadgeTipo
);
