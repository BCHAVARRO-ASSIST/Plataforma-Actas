namespace PlataformaActas.Api.Modules.Roles.Data.Entities;

public class Permiso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
