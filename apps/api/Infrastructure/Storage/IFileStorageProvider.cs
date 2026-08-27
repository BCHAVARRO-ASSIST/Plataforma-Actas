namespace PlataformaActas.Api.Infrastructure.Storage;

public interface IFileStorageProvider
{
    Task<string> SaveActaDocumentAsync(Guid actaId, string numeroActa, byte[] content, CancellationToken cancellationToken = default);
    Task<byte[]?> GetActaDocumentAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}
