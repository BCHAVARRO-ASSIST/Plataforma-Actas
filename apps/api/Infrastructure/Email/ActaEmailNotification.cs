namespace PlataformaActas.Api.Infrastructure.Email;

public sealed record ActaEmailNotification(
    IReadOnlyList<string> ToEmails,
    IReadOnlyList<string> CcEmails,
    IReadOnlyList<string> BccEmails,
    string DestinatarioNombre,
    string NumeroActa,
    string TipoActa,
    string EquipoSerial,
    string EquipoMarca,
    byte[] PdfBytes,
    DateTime? FechaFirmaUtc = null
);
