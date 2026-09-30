namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed record SyncResult(
    bool Success,
    string Message,
    int TotalProcessed,
    int Created,
    int Updated,
    int Unchanged,
    DateTimeOffset Timestamp,
    string? ErrorCode = null
);

public interface IGoogleDriveSyncService
{
    Task<SyncResult> SyncInventoryAsync(string? overrideFileId = null, string? overrideSheetName = null, CancellationToken cancellationToken = default);
    SyncResult? GetLastSyncStatus();
}
