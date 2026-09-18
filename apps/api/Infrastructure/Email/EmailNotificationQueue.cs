using System.Threading.Channels;

namespace PlataformaActas.Api.Infrastructure.Email;

/// <summary>
/// Implementación de cola de notificaciones en memoria basada en <see cref="Channel{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Desacoplamiento Asíncrono y Rendimiento HTTP:</b>
/// El despacho de correos electrónicos (vía Gmail API o SMTP institucional) involucra latencias de red,
/// negociación TLS y serialización de adjuntos PDF que pueden oscilar entre 500 ms y varios segundos.
/// Esta cola en memoria absorbe inmediatamente la notificación tras el commit transaccional de la firma,
/// permitiendo responder al cliente web o móvil en milisegundos (<c>&lt;200ms</c>) sin bloquear hilos del ThreadPool.
/// </para>
/// <para>
/// <b>Estrategia de Backpressure y Gestión de Memoria:</b>
/// Se configura un canal acotado (<see cref="BoundedChannelOptions"/>) con un límite estricto de 500 elementos y
/// modo <see cref="BoundedChannelFullMode.Wait"/>. Esto previene el agotamiento de memoria del contenedor (OOM)
/// ante eventuales caídas del servidor SMTP o picos imprevistos de firmas concurrentes.
/// Adicionalmente, <c>SingleWriter = false</c> admite concurrencia simultánea desde múltiples peticiones HTTP, mientras
/// <c>SingleReader = true</c> optimiza el canal para consumo secuencial por el servicio en segundo plano.
/// </para>
/// </remarks>
public sealed class EmailNotificationQueue : IEmailNotificationQueue
{
    private readonly Channel<ActaEmailNotification> _channel;

    public EmailNotificationQueue()
    {
        var options = new BoundedChannelOptions(500)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        };

        _channel = Channel.CreateBounded<ActaEmailNotification>(options);
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(ActaEmailNotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        await _channel.Writer.WriteAsync(notification, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ActaEmailNotification> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
