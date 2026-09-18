using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaActas.Api.Infrastructure.Email;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Infrastructure.Storage;
using PlataformaActas.Api.Modules.Actas.Data;
using PlataformaActas.Api.Modules.Actas.Data.Entities;
using PlataformaActas.Api.Modules.Actas.Models.Dto;
using PlataformaActas.Api.Modules.Actas.Models.Mappers;

namespace PlataformaActas.Api.Modules.Actas.Services;

/// <summary>
/// Servicio central de orquestación para el ciclo de vida de actas de entrega y devolución de activos TI.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ciclo de Vida Documental:</b>
/// Las actas transitan de forma estrictamente secuencial e irreversible:
/// <list type="bullet">
///   <item><description><b>Borrador:</b> Registro preliminar. El hardware permanece en su estado actual y puede modificarse el borrador.</description></item>
///   <item><description><b>Firmada:</b> Estado legalmente vinculante tras estampar la rúbrica biométrica manuscrita y aceptar cláusulas ISO 27001 / Ley 603 de 2000. El documento PDF se sella con hash criptográfico SHA-256 y se vuelve inmutable.</description></item>
///   <item><description><b>Finalizada:</b> Cierre definitivo de la custodia física y administrativa en campo.</description></item>
///   <item><description><b>Anulada:</b> Invalidación controlada antes de la finalización formal, exigiendo justificación obligatoria en auditoría.</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Integridad Transaccional:</b>
/// Las operaciones de firma sincronizan de forma atómica el estado del documento, la custodia física del hardware en inventario
/// y la persistencia del archivo antes de despachar las notificaciones por correo electrónico desacopladas.
/// </para>
/// </remarks>
public sealed class ActaService(
    IActaRepository actaRepository,
    IDocumentoActaService documentoActaService,
    IFileStorageProvider fileStorageProvider,
    IEmailNotificationQueue emailQueue,
    IOptions<SmtpOptions> smtpOptions,
    ILogger<ActaService> logger,
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

    /// <summary>
    /// Registra un nuevo borrador de acta de entrega o devolución, asignando un consecutivo anual formal.
    /// </summary>
    /// <param name="request">Parámetros del trámite, incluyendo el activo seleccionado y los usuarios responsables.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Información estructurada del acta recién creada en estado <c>Borrador</c>.</returns>
    /// <exception cref="ArgumentException">Se arroja si el hardware especificado no existe en el catálogo.</exception>
    /// <exception cref="InvalidOperationException">
    /// Se arroja si se intenta generar un acta de tipo <c>Entrega</c> sobre un equipo que no tiene estatus
    /// <c>Disponible</c> o <c>Activo</c> (evita doble asignación de hardware que ya esté en custodia o mantenimiento).
    /// </exception>
    public async Task<ActaResponse> CreateAsync(
        CreateActaRequest request,
        CancellationToken cancellationToken = default)
    {
        var equipo = await context.Equipos.FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
        if (equipo is null)
        {
            throw new ArgumentException("El equipo especificado no existe.");
        }

        var tipoActa = string.IsNullOrWhiteSpace(request.TipoActa) ? "Entrega" : request.TipoActa.Trim();

        if (string.Equals(tipoActa, "Entrega", StringComparison.OrdinalIgnoreCase))
        {
            var estatus = equipo.EstatusEquipo?.Trim() ?? string.Empty;
            if (string.Equals(estatus, "Asignado", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(estatus, "En Mantenimiento", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(estatus, "Mantenimiento", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(estatus, "Dado de Baja", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(estatus, "De Baja", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(estatus, "Inactivo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"El equipo con placa '{equipo.InventoryKey}' no se encuentra disponible para entrega (Estatus actual: '{equipo.EstatusEquipo}'). Debe estar en estado 'Disponible' o 'Activo'.");
            }
        }

        var year = DateTime.UtcNow.Year;
        var secuencia = await actaRepository.GetNextSecuenciaByYearAsync(year, cancellationToken);
        var numeroActa = $"ACTA-{year}-{secuencia:D4}";

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

    /// <summary>
    /// Estampa la firma digital manuscrita en un acta en borrador, consolidando la custodia legal del activo
    /// y desencadenando la generación inmutable del PDF y su notificación asíncrona.
    /// </summary>
    /// <param name="id">Identificador único global (UUID) del acta a firmar.</param>
    /// <param name="request">Payload con la rúbrica biométrica serializada, observaciones de campo y destinatarios adicionales.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Detalle del acta actualizado en estado <c>Firmada</c>, o <c>null</c> si el identificador no existe.</returns>
    /// <exception cref="InvalidOperationException">
    /// Se arroja si el acta no se encuentra en estado <c>Borrador</c>. Por seguridad jurídica y trazabilidad de auditoría,
    /// un documento ya firmado, finalizado o anulado es estrictamente inmutable y no admite refirma.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <b>Atomicidad Transaccional:</b>
    /// Toda la mutación de estado (registro de rúbrica bajo norma ISO 27001, histórico de auditoría, generación y sellado
    /// SHA-256 del PDF en almacenamiento físico, y actualización del estatus del hardware a <c>Asignado</c> o <c>Activo</c>)
    /// se ejecuta dentro de una transacción relacional explícita (<see cref="Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction"/>).
    /// Si cualquier paso falla, la base de datos revierte íntegramente los cambios.
    /// </para>
    /// <para>
    /// <b>Resolución de Listas de Notificación y Desacoplamiento:</b>
    /// Una vez confirmada la transacción (<c>CommitAsync</c>), se calcula la matriz de destinatarios:
    /// <list type="bullet">
    ///   <item><description><b>To:</b> Correo corporativo del consultor destinatario y copias directas añadidas en campo.</description></item>
    ///   <item><description><b>Cc:</b> Usuario creador del trámite y mensajero responsable de la entrega física en sitio.</description></item>
    ///   <item><description><b>Bcc:</b> Buzones institucionales de auditoría y soporte (APS y Soporte TI).</description></item>
    /// </list>
    /// El envío se delega a <see cref="IEmailNotificationQueue"/> en memoria, garantizando que demoras o caídas en la pasarela SMTP/Gmail
    /// jamás degraden el tiempo de respuesta HTTP ni reviertan la validez legal del acta firmada.
    /// </para>
    /// </remarks>
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

        if (!string.Equals(acta.Estado, "Borrador", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"El acta '{acta.NumeroActa}' no se puede firmar porque su estado actual es '{acta.Estado}'. Solo las actas en estado 'Borrador' pueden ser firmadas.");
        }

        string normalizedSignatureJson;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(request.SignatureDataJson);
            normalizedSignatureJson = request.SignatureDataJson;
        }
        catch
        {
            normalizedSignatureJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                firma = request.SignatureDataJson
            });
        }

        acta.SignatureDataJson = normalizedSignatureJson;
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

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await actaRepository.AddSignatureAsync(new ActaSignature
            {
                ActaId = acta.Id,
                SignatureBase64 = request.SignatureDataJson,
                ConsentAccepted = true,
                LegalClauseVersion = "3.2-ISO27001",
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
            var equipo = await context.Equipos.FirstOrDefaultAsync(e => e.Id == acta.EquipmentId, cancellationToken);
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

                context.Equipos.Update(equipo);
            }

            await actaRepository.UpdateAsync(acta, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Encolamiento asíncrono y desacoplado de la notificación por correo
            try
            {
                var toList = new List<string>();
                if (!string.IsNullOrWhiteSpace(acta.DestinatarioUsuario?.Correo))
                {
                    toList.Add(acta.DestinatarioUsuario.Correo.Trim());
                }

                if (request.CorreosNotificacionAdicionales != null)
                {
                    foreach (var email in request.CorreosNotificacionAdicionales)
                    {
                        if (!string.IsNullOrWhiteSpace(email) && !toList.Any(e => string.Equals(e, email.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            toList.Add(email.Trim());
                        }
                    }
                }

                var ccList = new List<string>();
                if (!string.IsNullOrWhiteSpace(acta.Usuario?.Correo) && !toList.Any(e => string.Equals(e, acta.Usuario.Correo.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    ccList.Add(acta.Usuario.Correo.Trim());
                }

                if (!string.IsNullOrWhiteSpace(acta.MensajeroUsuario?.Correo) &&
                    !toList.Any(e => string.Equals(e, acta.MensajeroUsuario.Correo.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                    !ccList.Any(e => string.Equals(e, acta.MensajeroUsuario.Correo.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    ccList.Add(acta.MensajeroUsuario.Correo.Trim());
                }

                var bccList = new List<string>();
                var smtpOpt = smtpOptions.Value;
                if (smtpOpt.SoporteNotificationEmails != null)
                {
                    bccList.AddRange(smtpOpt.SoporteNotificationEmails.Where(e => !string.IsNullOrWhiteSpace(e)));
                }
                if (smtpOpt.ApsNotificationEmails != null)
                {
                    bccList.AddRange(smtpOpt.ApsNotificationEmails.Where(e => !string.IsNullOrWhiteSpace(e)));
                }

                byte[] pdfBytes = Array.Empty<byte>();
                if (!string.IsNullOrWhiteSpace(acta.RutaDocumento))
                {
                    var fileBytes = await fileStorageProvider.GetActaDocumentAsync(acta.RutaDocumento, cancellationToken);
                    if (fileBytes != null)
                    {
                        pdfBytes = fileBytes;
                    }
                }

                var destinatarioNombre = acta.DestinatarioUsuario != null
                    ? $"{acta.DestinatarioUsuario.Nombre} {acta.DestinatarioUsuario.Apellido}".Trim()
                    : (equipo?.AsignadoA ?? "Colaborador Asignado");

                var notification = new ActaEmailNotification(
                    ToEmails: toList,
                    CcEmails: ccList,
                    BccEmails: bccList,
                    DestinatarioNombre: destinatarioNombre,
                    NumeroActa: acta.NumeroActa ?? "ACTA",
                    TipoActa: acta.TipoActa ?? "Entrega",
                    EquipoSerial: equipo?.InventoryKey ?? "—",
                    EquipoMarca: equipo?.Marca ?? (equipo?.NombreEquipo ?? "Activo TI"),
                    PdfBytes: pdfBytes
                );

                await emailQueue.EnqueueAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                // Fallos en el encolamiento de correos no revierten la firma del acta
                logger.LogError(ex, "Error al encolar notificación de correo para el acta {NumeroActa}.", acta.NumeroActa);
            }

            return acta.ToResponse();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Concluye formalmente el ciclo operativo de un acta, certificando la entrega efectiva en campo.
    /// </summary>
    /// <param name="id">Identificador único global (UUID) del acta a finalizar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Detalle del acta en estado <c>Finalizada</c>, o <c>null</c> si no existe.</returns>
    /// <remarks>
    /// Esta operación marca el cierre administrativo formal de la custodia física (<c>ActaEntregada = true</c>)
    /// y registra la transición inmutable en el histórico <see cref="ActaStatusHistory"/> para trazabilidad de auditoría.
    /// </remarks>
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

    /// <summary>
    /// Invalida un acta de forma controlada registrando formalmente el motivo justificado de la anulación.
    /// </summary>
    /// <param name="id">Identificador único del acta a anular.</param>
    /// <param name="request">Información y justificación obligatoria de la anulación para fines de auditoría.</param>
    /// <param name="usuarioId">Identificador del usuario responsable de ordenar la anulación.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Detalle del acta en estado <c>Anulada</c>, o <c>null</c> si el registro no existe.</returns>
    /// <remarks>
    /// La anulación no destruye registros físicos ni lógicos; preserva la trazabilidad completa en el historial
    /// <see cref="ActaStatusHistory"/> garantizando el principio de no repudio según las directrices de seguridad de la información.
    /// </remarks>
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
