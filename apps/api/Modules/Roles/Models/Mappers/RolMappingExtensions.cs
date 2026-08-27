using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Roles.Models.Dto;

namespace PlataformaActas.Api.Modules.Roles.Models.Mappers;

public static class RolMappingExtensions
{
    public static RolResponse ToResponse(this Rol rol)
    {
        return new RolResponse(
            rol.Id,
            rol.Nombre,
            rol.Descripcion,
            rol.Activo,
            rol.FechaCreacion
        );
    }

    public static RolDetalleResponse ToDetalleResponse(this Rol rol)
    {
        var permisos = rol.RolPermisos
            .Where(rp => rp.Permiso != null)
            .Select(rp => rp.Permiso!.ToResponse())
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Nombre)
            .ToList();

        return new RolDetalleResponse(
            rol.Id,
            rol.Nombre,
            rol.Descripcion,
            rol.Activo,
            rol.FechaCreacion,
            permisos
        );
    }

    public static PermisoResponse ToResponse(this Permiso permiso)
    {
        return new PermisoResponse(
            permiso.Id,
            permiso.Nombre,
            permiso.Modulo,
            permiso.Descripcion,
            permiso.Activo
        );
    }
}
