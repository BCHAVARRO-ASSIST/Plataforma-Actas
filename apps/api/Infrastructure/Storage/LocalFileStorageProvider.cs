using Microsoft.AspNetCore.Hosting;

namespace PlataformaActas.Api.Infrastructure.Storage;

public sealed class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly string _storageRoot;

    public LocalFileStorageProvider(IWebHostEnvironment environment)
    {
        // Root path is either local project root storage/ or content root storage/
        var solutionStorage = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "storage", "documentos"));
        if (Directory.Exists(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "storage"))))
        {
            _storageRoot = solutionStorage;
        }
        else
        {
            _storageRoot = Path.Combine(environment.ContentRootPath, "storage", "documentos");
        }

        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveActaDocumentAsync(
        Guid actaId,
        string numeroActa,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var sanitizedNumero = SanitizeFileName(numeroActa);
        var actaFolder = Path.Combine(_storageRoot, actaId.ToString());
        Directory.CreateDirectory(actaFolder);

        var fileName = $"Acta_{sanitizedNumero}.pdf";
        var fullPath = Path.Combine(actaFolder, fileName);

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        // Return relative path from storage root
        return Path.Combine("storage", "documentos", actaId.ToString(), fileName).Replace('\\', '/');
    }

    public async Task<byte[]?> GetActaDocumentAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(relativePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private string ResolveFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("storage/documentos/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["storage/documentos/".Length..];
        }
        else if (normalized.StartsWith("Storage/Documentos/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["Storage/Documentos/".Length..];
        }

        return Path.Combine(_storageRoot, normalized);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalids = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalids.Contains(c) ? '_' : c)).Trim();
    }
}
