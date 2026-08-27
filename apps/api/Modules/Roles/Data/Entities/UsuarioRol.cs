namespace PlataformaActas.Api.Modules.Roles.Data.Entities;

public class UsuarioRol
{
    public int UsuarioId { get; set; }
    public PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario? Usuario { get; set; }

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
