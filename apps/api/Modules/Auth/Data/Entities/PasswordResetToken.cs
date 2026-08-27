using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Auth.Data.Entities;

public class PasswordResetToken
{
    public long Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public DateTime? FechaUso { get; set; }
}
