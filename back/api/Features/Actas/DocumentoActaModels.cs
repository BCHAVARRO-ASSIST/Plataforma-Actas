namespace PlataformaActas.Api.Features.Actas;

public sealed record DocumentoActaResult(
    byte[] PdfBytes,
    string FileName,
    string ContentType,
    string? RutaDocumento,
    string? HashSha256,
    DateTimeOffset FechaGeneracion
);

public sealed record GenerarDocumentoResponse(
    Guid ActaId,
    string NumeroActa,
    string RutaDocumento,
    string? HashSha256,
    DateTimeOffset FechaGeneracion,
    long TamanoBytes
);
