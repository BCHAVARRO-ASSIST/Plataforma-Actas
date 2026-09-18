namespace PlataformaActas.Api.Infrastructure.Email;

/// <summary>
/// Contrato para la cola en memoria de notificaciones de actas por correo electrónico.
/// </summary>
/// <remarks>
/// Desacopla la invocación HTTP sincrónica de la entrega efectiva de correos con adjuntos PDF pesados,
/// protegiendo el pipeline web contra demoras de transporte y fallos de proveedores de mensajería externa.
/// </remarks>
public interface IEmailNotificationQueue
{
    /// <summary>
    /// Encola una notificación de acta firmada para su posterior procesamiento en segundo plano.
    /// </summary>
    /// <param name="notification">Carga útil con destinatarios, metadatos y bytes del PDF adjunto.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Estructura <see cref="ValueTask"/> que representa la operación de escritura en el canal.</returns>
    ValueTask EnqueueAsync(ActaEmailNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Proporciona un flujo asíncrono continuo de notificaciones para el consumidor en segundo plano.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para detener la lectura del flujo.</param>
    /// <returns>Secuencia asíncrona <see cref="IAsyncEnumerable{T}"/> de notificaciones.</returns>
    IAsyncEnumerable<ActaEmailNotification> ReadAllAsync(CancellationToken cancellationToken = default);
}
