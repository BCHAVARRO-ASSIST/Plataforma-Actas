using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data;

public sealed class ActaRepository : IActaRepository
{
    private readonly PlataformaActasDbContext _context;

    public ActaRepository(PlataformaActasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Acta>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Actas
            .AsNoTracking()
            .Include(a => a.Equipment)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Acta>> GetByMensajeroAsync(int mensajeroId, CancellationToken cancellationToken = default)
    {
        return await _context.Actas
            .AsNoTracking()
            .Include(a => a.Equipment)
            .Where(a => a.MensajeroUsuarioId == mensajeroId)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<Acta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Actas
            .Include(a => a.Equipment)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Acta?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Actas
            .AsNoTracking()
            .Include(a => a.Equipment)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.MensajeroUsuario)
            .Include(a => a.ActaOrigen)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<int> GetNextSecuenciaByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var count = await _context.Actas
            .Where(a => a.FechaCreacion.Year == year)
            .CountAsync(cancellationToken);

        return count + 1;
    }

    public async Task<Acta> AddAsync(Acta acta, CancellationToken cancellationToken = default)
    {
        await _context.Actas.AddAsync(acta, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return acta;
    }

    public async Task UpdateAsync(Acta acta, CancellationToken cancellationToken = default)
    {
        _context.Actas.Update(acta);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStatusHistoryAsync(ActaStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _context.ActaStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddSignatureAsync(ActaSignature signature, CancellationToken cancellationToken = default)
    {
        await _context.ActaSignatures.AddAsync(signature, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
