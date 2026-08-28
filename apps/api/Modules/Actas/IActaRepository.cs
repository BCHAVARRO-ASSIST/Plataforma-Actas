using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data;

public interface IActaRepository
{
    Task<IReadOnlyList<Acta>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Acta>> GetByMensajeroAsync(int mensajeroId, CancellationToken cancellationToken = default);
    Task<Acta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Acta?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Acta> AddAsync(Acta acta, CancellationToken cancellationToken = default);
    Task UpdateAsync(Acta acta, CancellationToken cancellationToken = default);
    Task<int> GetNextSecuenciaByYearAsync(int year, CancellationToken cancellationToken = default);
    Task AddStatusHistoryAsync(ActaStatusHistory history, CancellationToken cancellationToken = default);
    Task AddSignatureAsync(ActaSignature signature, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
