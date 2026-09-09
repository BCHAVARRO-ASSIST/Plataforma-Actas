using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Actas.Data;
using PlataformaActas.Api.Modules.Actas.Data.Entities;
using PlataformaActas.Api.Modules.Actas.Models.Dto;
using PlataformaActas.Api.Modules.Actas.Models.Mappers;

namespace PlataformaActas.Api.Modules.Actas.Services;

public sealed class ActaService(
    IActaRepository actaRepository,
    IDocumentoActaService documentoActaService,
    PlataformaActasDbContext context) : IActaService
{

    public async Task<IReadOnlyList<ActaResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await actaRepository.GetAllAsync(cancellationToken);
        return list.Select(a => a.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ActaResponse>> GetByMensajeroAsync(int mensajeroId, CancellationToken cancellationToken = default)
    {
        var list = await actaRepository.GetByMensajeroAsync(mensajeroId, cancellationToken);
        return list.Select(a => a.ToResponse()).ToList();
    }

    public async Task<ActaDetailResponse?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var acta = await actaRepository.GetDetailByIdAsync(id, cancellationToken);
        return acta?.ToDetailResponse();
    }

    public async Task<ActaResponse> CreateAsync(
        CreateActaRequest request,
        CancellationToken cancellationToken = default)
    {
        var equipo = await context.Equipment.FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
        if (equipo is null)
        {
            throw new ArgumentException("El equipo especificado no existe.");
        }

        var year = DateTime.UtcNow.Year;
        var secuencia = await actaRepository.GetNextSecuenciaByYearAsync(year, cancellationToken);
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

        var created = await actaRepository.AddAsync(acta, cancellationToken);

        await actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = created.Id,
            EstadoAnterior = "Ninguno",
            EstadoNuevo = "Borrador",
            UsuarioId = request.UsuarioId,
            Comentario = "Creación inicial de acta",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        return created.ToResponse();
    }

    public async Task<ActaResponse?> SignAsync(
        Guid id,
        FirmarActaRequest request,
        CancellationToken cancellationToken = default)
    {
        var acta = await actaRepository.GetByIdAsync(id, cancellationToken);
        if (acta is null)
        {
            return null;
        }

        acta.SignatureDataJson = request.SignatureDataJson;
        acta.FechaFirma = DateTimeOffset.UtcNow;
        acta.Estado = "Firmada";

        if (!string.IsNullOrWhiteSpace(request.Observacion))
        {
            try
            {
                var formData = string.IsNullOrWhiteSpace(acta.FormDataJson)
                    ? new Dictionary<string, object>()
                    : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(acta.FormDataJson) ?? new();
                formData["observacion"] = request.Observacion.Trim();
                acta.FormDataJson = System.Text.Json.JsonSerializer.Serialize(formData);
            }
            catch
            {
                acta.FormDataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    tipo = acta.TipoActa,
                    observacion = request.Observacion.Trim()
                });
            }
        }

        await actaRepository.AddSignatureAsync(new ActaSignature
        {
            ActaId = acta.Id,
            SignatureBase64 = request.SignatureDataJson,
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        await actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = acta.Id,
            EstadoAnterior = "Borrador",
            EstadoNuevo = "Firmada",
            UsuarioId = acta.UsuarioId,
            Comentario = "Firma manuscrita capturada",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Generar documento oficial PDF
        var docResponse = await documentoActaService.GenerarYGuardarPdfAsync(acta.Id, cancellationToken);
        acta.RutaDocumento = docResponse.RutaDocumento;
        acta.HashSha256 = docResponse.HashSha256;

        // Sincronización automática de inventario
        var equipo = await context.Equipment.FirstOrDefaultAsync(e => e.Id == acta.EquipmentId, cancellationToken);
        if (equipo != null)
        {
            if (acta.TipoActa == "Entrega")
            {
                equipo.EstatusEquipo = "Asignado";
                equipo.ActaEntregada = true;
                if (acta.DestinatarioUsuario != null)
                {
                    equipo.AsignadoA = $"{acta.DestinatarioUsuario.Nombre} {acta.DestinatarioUsuario.Apellido}".Trim();
                }
            }
            else if (acta.TipoActa == "Devolución" || acta.TipoActa == "Devolucion")
            {
                equipo.EstatusEquipo = "Activo";
                equipo.ActaEntregada = false;
                equipo.AsignadoA = null;
            }

            context.Equipment.Update(equipo);
        }

        await actaRepository.UpdateAsync(acta, cancellationToken);
        return acta.ToResponse();
    }

    public async Task<ActaResponse?> FinalizeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var acta = await actaRepository.GetByIdAsync(id, cancellationToken);
        if (acta is null)
        {
            return null;
        }

        var estadoPrevio = acta.Estado;
        acta.Estado = "Finalizada";
        acta.FechaFinalizacion = DateTimeOffset.UtcNow;
        acta.ActaEntregada = true;

        await actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = acta.Id,
            EstadoAnterior = estadoPrevio,
            EstadoNuevo = "Finalizada",
            UsuarioId = acta.UsuarioId,
            Comentario = "Acta finalizada exitosamente",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        await actaRepository.UpdateAsync(acta, cancellationToken);
        return acta.ToResponse();
    }

    public async Task<ActaResponse?> AnularAsync(
        Guid id,
        AnularActaRequest request,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var acta = await actaRepository.GetByIdAsync(id, cancellationToken);
        if (acta is null)
        {
            return null;
        }

        var estadoPrevio = acta.Estado;
        acta.Estado = "Anulada";
        acta.FechaAnulacion = DateTimeOffset.UtcNow;
        acta.MotivoAnulacion = request.Motivo;

        await actaRepository.AddStatusHistoryAsync(new ActaStatusHistory
        {
            ActaId = acta.Id,
            EstadoAnterior = estadoPrevio,
            EstadoNuevo = "Anulada",
            UsuarioId = usuarioId ?? acta.UsuarioId,
            Comentario = $"Anulación: {request.Motivo}",
            FechaCreacion = DateTimeOffset.UtcNow
        }, cancellationToken);

        await actaRepository.UpdateAsync(acta, cancellationToken);
        return acta.ToResponse();
    }
}
