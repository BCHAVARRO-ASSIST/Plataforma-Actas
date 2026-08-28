using PlataformaActas.Api.Modules.Actas.Models.Dto;

namespace PlataformaActas.Api.Modules.Actas.Services;

public interface IActaService
{
    Task<IReadOnlyList<ActaResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActaResponse>> GetByMensajeroAsync(int mensajeroId, CancellationToken cancellationToken = default);
    Task<ActaDetailResponse?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActaResponse> CreateAsync(CreateActaRequest request, CancellationToken cancellationToken = default);
    Task<ActaResponse?> SignAsync(Guid id, FirmarActaRequest request, CancellationToken cancellationToken = default);
    Task<ActaResponse?> FinalizeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActaResponse?> AnularAsync(Guid id, AnularActaRequest request, int? usuarioId = null, CancellationToken cancellationToken = default);
}
