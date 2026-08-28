namespace PlataformaActas.Api.Modules.Dashboard.Models.Dto
{
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

    public sealed record StatusCountItem(
        string Estado,
        int Cantidad,
        double Porcentaje,
        string Color
    );

    public sealed record MessengerDeliveryItem(
        int? MensajeroId,
        string MensajeroNombre,
        int TotalAsignadas,
        int TotalEntregadas,
        int TotalPendientes
    );

    public sealed record TimelinePointItem(
        string Fecha,
        int TotalCreadas,
        int TotalFirmadas,
        int TotalEntregas,
        int TotalDevoluciones
    );

    public sealed record MaintenanceStatsResponse(
        int Total,
        int Preventivos,
        int Correctivos,
        int Concluidos,
        int Pendientes,
        IReadOnlyList<StatusCountItem> Tipos
    );

    public sealed record RecentActivityItem(
        string Id,
        string TipoEvento,
        string Titulo,
        string Descripcion,
        string Responsable,
        DateTimeOffset Fecha,
        string Estado,
        string BadgeTipo
    );
}
