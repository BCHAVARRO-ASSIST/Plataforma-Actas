using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlataformaActas.Api.Infrastructure.Email;

/// <summary>
/// Proceso de fondo continuo (Worker) encargado del consumo y despacho asíncrono de notificaciones de actas.
/// </summary>
/// <remarks>
/// <para>
/// <b>Patrón de Aislamiento y No Bloqueo:</b>
/// Al procesar las notificaciones fuera del pipeline de solicitudes HTTP, se aísla por completo la experiencia
/// de usuario ante demoras de red externas o congestión temporal en los servidores de correo (Gmail / SMTP).
/// </para>
/// <para>
/// <b>Resolución de Servicios con Alcance (Scoped Dependencies):</b>
/// Como este servicio opera con ciclo de vida <see cref="ServiceLifetime.Singleton"/> durante toda la ejecución de la aplicación,
/// utiliza <see cref="IServiceScopeFactory"/> para instanciar un alcance temporal (<see cref="IServiceScope"/>) por cada
/// notificación procesada. Esto previene problemas de dependencias cautivas (<i>Captive Dependencies</i>) y asegura que
/// los recursos de red y configuración se liberen adecuadamente tras cada despacho.
/// </para>
/// <para>
/// <b>Resiliencia y Cierre Controlado (Graceful Shutdown):</b>
/// El bucle de lectura intercepta <see cref="OperationCanceledException"/> para respetar la señal de parada del contenedor
/// (SIGTERM) sin corromper envíos en curso. Cualquier error en el envío de un correo individual es capturado y registrado
/// en los logs de auditoría sin interrumpir el procesamiento del resto de la cola.
/// </para>
/// </remarks>
public sealed class EmailNotificationBackgroundService : BackgroundService
{
    private readonly IEmailNotificationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailNotificationBackgroundService> _logger;

    public EmailNotificationBackgroundService(
        IEmailNotificationQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailNotificationBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Ciclo de ejecución principal del worker de mensajería.
    /// </summary>
    /// <param name="stoppingToken">Token emitido por el host durante la detención de la aplicación.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de segundo plano para notificaciones por correo iniciado.");

        await foreach (var notification in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();

                await sender.SendActaNotificationAsync(notification, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al procesar notificación por correo para el acta {NumeroActa}.", notification.NumeroActa);
            }
        }

        _logger.LogInformation("Servicio de segundo plano para notificaciones por correo detenido.");
    }
}
