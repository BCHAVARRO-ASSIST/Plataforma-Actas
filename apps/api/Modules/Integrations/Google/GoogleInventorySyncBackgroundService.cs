using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed class GoogleInventorySyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GoogleIntegrationOptions _options;
    private readonly ILogger<GoogleInventorySyncBackgroundService> _logger;

    public GoogleInventorySyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<GoogleIntegrationOptions> options,
        ILogger<GoogleInventorySyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Google Inventory Sync Background Service está deshabilitado.");
            return;
        }

        _logger.LogInformation("Google Inventory Sync Background Service iniciado. Intervalo: cada {Minutes} minutos.", _options.SyncIntervalMinutes);

        // Esperar 10 segundos al iniciar la app antes del primer ciclo
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Ejecutando ciclo automático de sincronización de inventario...");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IGoogleDriveSyncService>();
                    var result = await syncService.SyncInventoryAsync(stoppingToken);
                    _logger.LogInformation("Ciclo automático finalizado: {Message}", result.Message);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error no controlado en el ciclo automático de sincronización.");
            }

            var delayMinutes = Math.Max(1, _options.SyncIntervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }
}
