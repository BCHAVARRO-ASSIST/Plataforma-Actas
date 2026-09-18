namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed record SyncResult(
    bool Success,
    string Message,
    int TotalProcessed,
    int Created,
    int Updated,
    int Unchanged,
    DateTimeOffset Timestamp
);

public interface IGoogleDriveSyncService
{
    Task<SyncResult> SyncInventoryAsync(CancellationToken cancellationToken = default);
    SyncResult? GetLastSyncStatus();
}
