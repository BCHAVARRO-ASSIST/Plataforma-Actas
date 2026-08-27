using PlataformaActas.Api.Modules.Reportes.Models.Dto;

namespace PlataformaActas.Api.Modules.Reportes.Services;

public interface IReportesService
{
    Task<IReadOnlyList<EquiposReportItem>> GetEquiposReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActasReportItem>> GetActasReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntregasReportItem>> GetEntregasReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DevolucionesReportItem>> GetDevolucionesReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);
}
