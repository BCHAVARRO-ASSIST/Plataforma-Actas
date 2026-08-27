using PlataformaActas.Api.Modules.Actas.Models.Dto;

namespace PlataformaActas.Api.Modules.Actas.Services;

public interface IDocumentoActaService
{
    byte[] GenerarPdfBytes(ActaDetailResponse acta);
    Task<GenerarDocumentoResponse> GenerarYGuardarPdfAsync(Guid actaId, CancellationToken cancellationToken = default);
    Task<DocumentoActaResult> ObtenerOVisualizarPdfAsync(Guid actaId, CancellationToken cancellationToken = default);
}
