namespace PlataformaActas.Api.Modules.Actas.Data.Entities;

public class ActaSignature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActaId { get; set; }
    public Acta Acta { get; set; } = null!;
    public string SignatureBase64 { get; set; } = string.Empty;
    public bool ConsentAccepted { get; set; }
    public string? LegalClauseVersion { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}
