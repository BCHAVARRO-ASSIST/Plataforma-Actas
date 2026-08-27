using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Entities;

public class ActaStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActaId { get; set; }
    public Acta Acta { get; set; } = null!;
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string? Comentario { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}
