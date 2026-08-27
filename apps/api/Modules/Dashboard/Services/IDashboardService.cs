using PlataformaActas.Api.Modules.Dashboard.Models.Dto;

namespace PlataformaActas.Api.Modules.Dashboard.Services;

public interface IDashboardService
{
    Task<DashboardKpiResponse> GetKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusCountItem>> GetEquipmentStatusChartAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusCountItem>> GetActasStatusChartAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessengerDeliveryItem>> GetDeliveriesByMessengerAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimelinePointItem>> GetTimelineAsync(string? range, CancellationToken cancellationToken = default);
    Task<MaintenanceStatsResponse> GetMaintenanceStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentActivityItem>> GetRecentActivityAsync(int limit, CancellationToken cancellationToken = default);
}
