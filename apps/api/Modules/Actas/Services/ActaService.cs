using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Actas.Data;
using PlataformaActas.Api.Modules.Actas.Data.Entities;
using PlataformaActas.Api.Modules.Actas.Models.Dto;
using PlataformaActas.Api.Modules.Actas.Models.Mappers;

namespace PlataformaActas.Api.Modules.Actas.Services;

public sealed class ActaService : IActaService
{
    private readonly IActaRepository _actaRepository;
    private readonly PlataformaActasDbContext _context;
    private readonly IDocumentoActaService _documentoActaService;

    public ActaService(
        IActaRepository actaRepository,
        PlataformaActasDbContext context,
        IDocumentoActaService documentoActaService)
    {
        _actaRepository = actaRepository;
        _context = context;
        _documentoActaService = documentoActaService;
    }

    public async Task<IReadOnlyList<ActaResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var actas = await _actaRepository.GetAllAsync(cancellationToken);
        return actas.Select(a => a.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ActaResponse>> GetByMensajeroAsync(int mensajeroId, CancellationToken cancellationToken = default)
    {
        var actas = await _actaRepository.GetByMensajeroAsync(mensajeroId, cancellationToken);
        return actas.Select(a => a.ToResponse()).ToList();
    }

    public async Task<ActaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var acta = await _actaRepository.GetByIdAsync(id, cancellationToken);
        return acta?.ToResponse();
    }

    public async Task<ActaDetailResponse?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var acta = await _actaRepository.GetDetailByIdAsync(id, cancellationToken);
        return acta?.ToDetailResponse();
    }

    public async Task<ActaResponse> CreateAsync(CreateActaRequest request, CancellationToken cancellationToken = default)
    {
        var equipo = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
        if (equipo is null)
        {
            throw new ArgumentException("El equipo especificado no existe.");
        }

        var year = DateTime.UtcNow.Year;
        var secuencia = await _actaRepository.GetNextSecuenciaByYearAsync(year, cancellationToken);
        var numeroActa = $"ACTA-{year}-{secuencia:D4}";

        var tipoActa = string.IsNullOrWhiteSpace(request.TipoActa) ? "Entrega" : request.TipoActa.Trim();

        var acta = new Acta
        {
            Id = Guid.NewGuid(),
            NumeroActa = numeroActa,
            EquipmentId = request.EquipmentId,
            UsuarioId = request.UsuarioId,
            DestinatarioUsuarioId = request.DestinatarioUsuarioId,
            MensajeroUsuarioId = request.MensajeroUsuarioId,
            ActaEntregada = request.ActaEntregada,
            Estado = "Borrador",
            TipoActa = tipoActa,
            ActaOrigenId = request.ActaOrigenId,
            DeviceId = request.DeviceId,
            FormDataJson = request.FormDataJson,
            FechaCreacion = DateTimeOffset.UtcNow
        };

        var created = await _actaRepository.AddAsync(acta, cancellationToken);

        await _actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = created.Id,
            EstadoAnterior = "Ninguno",
            EstadoNuevo = "Borrador",
            UsuarioId = request.UsuarioId,
            Comentario = $"Acta creada en estado Borrador ({tipoActa})",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Pre-generate draft PDF document
        try
        {
            await _documentoActaService.GenerarYGuardarPdfAsync(created.Id, cancellationToken);
        }
        catch
        {
            // Logging can capture non-blocking draft PDF pre-generation
        }

        return created.ToResponse();
    }

    public async Task<ActaResponse?> SignAsync(
        Guid id,
        FirmarActaRequest request,
        CancellationToken cancellationToken = default)
    {
        var acta = await _actaRepository.GetByIdAsync(id, cancellationToken);
        if (acta is null)
        {
            return null;
        }

        if (acta.Estado == "Anulada")
        {
            throw new InvalidOperationException("No se puede firmar un acta anulada.");
        }

        var estadoAnterior = acta.Estado;
        acta.SignatureDataJson = request.SignatureDataJson;
        acta.Estado = "Firmada";
        acta.FechaFirma = DateTimeOffset.UtcNow;
        acta.ActaEntregada = true;

        await _actaRepository.AddSignatureAsync(new ActaSignature
        {
            ActaId = acta.Id,
            SignatureBase64 = request.SignatureDataJson,
            ConsentAccepted = true,
            LegalClauseVersion = "3.2-ISO27001",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Sincronización automática de inventario
        var equipo = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == acta.EquipmentId, cancellationToken);
        if (equipo is not null)
        {
            var isDevolucion = string.Equals(acta.TipoActa, "Devolución", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(acta.TipoActa, "Devolucion", StringComparison.OrdinalIgnoreCase);

            if (isDevolucion)
            {
                equipo.EstatusEquipo = "Activo";
                equipo.ActaEntregada = false;
                equipo.AsignadoA = null;
            }
            else
            {
                equipo.EstatusEquipo = "Asignado";
                equipo.ActaEntregada = true;
                if (acta.DestinatarioUsuarioId.HasValue)
                {
                    var destinatario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == acta.DestinatarioUsuarioId.Value, cancellationToken);
                    if (destinatario is not null)
                    {
                        equipo.AsignadoA = $"{destinatario.Nombre} {destinatario.Apellido}";
                    }
                }
            }
        }

        await _actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = acta.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = "Firmada",
            UsuarioId = acta.DestinatarioUsuarioId ?? acta.UsuarioId,
            Comentario = "Acta firmada formalmente por el receptor.",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _actaRepository.SaveChangesAsync(cancellationToken);

        // Generar y sellar PDF oficial con hash SHA-256
        await _documentoActaService.GenerarYGuardarPdfAsync(acta.Id, cancellationToken);

        return acta.ToResponse();
    }

    public async Task<ActaResponse?> FinalizeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var acta = await _actaRepository.GetByIdAsync(id, cancellationToken);
        if (acta is null)
        {
            return null;
        }

        if (acta.Estado != "Firmada")
        {
            throw new InvalidOperationException("Solo se pueden finalizar actas que ya han sido firmadas.");
        }

        var estadoAnterior = acta.Estado;
        acta.Estado = "Finalizada";
        acta.FechaFinalizacion = DateTimeOffset.UtcNow;

        await _actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = acta.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = "Finalizada",
            UsuarioId = acta.UsuarioId,
            Comentario = "Acta finalizada y archivada formalmente.",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _actaRepository.SaveChangesAsync(cancellationToken);
        return acta.ToResponse();
    }

    public async Task<ActaResponse?> AnularAsync(
        Guid id,
        AnularActaRequest request,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var acta = await _actaRepository.GetByIdAsync(id, cancellationToken);
        if (acta is null)
        {
            return null;
        }

        if (acta.Estado == "Anulada")
        {
            throw new InvalidOperationException("El acta ya se encuentra anulada.");
        }

        var estadoAnterior = acta.Estado;
        acta.Estado = "Anulada";
        acta.FechaAnulacion = DateTimeOffset.UtcNow;
        acta.MotivoAnulacion = request.Motivo;

        // Si el equipo estaba asignado por esta acta, revertir estado
        var equipo = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == acta.EquipmentId, cancellationToken);
        if (equipo is not null && equipo.EstatusEquipo == "Asignado")
        {
            equipo.EstatusEquipo = "Activo";
            equipo.ActaEntregada = false;
        }

        await _actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = acta.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = "Anulada",
            UsuarioId = usuarioId ?? acta.UsuarioId,
            Comentario = $"Acta anulada. Motivo: {request.Motivo}",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _actaRepository.SaveChangesAsync(cancellationToken);
        return acta.ToResponse();
    }
}
