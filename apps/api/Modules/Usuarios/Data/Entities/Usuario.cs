using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Usuarios.Data.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string UsuarioRed { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Proceso { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public string? PasswordHash { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
