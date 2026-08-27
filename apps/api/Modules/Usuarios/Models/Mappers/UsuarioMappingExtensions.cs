using PlataformaActas.Api.Modules.Usuarios.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Models.Dto;

namespace PlataformaActas.Api.Modules.Usuarios.Models.Mappers;

public static class UsuarioMappingExtensions
{
    public static UsuarioResponse ToResponse(this Usuario usuario)
    {
        var rolActivo = usuario.UsuarioRoles
            .Select(ur => ur.Rol)
            .FirstOrDefault(r => r != null && r.Activo);

        return new UsuarioResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.UsuarioRed,
            usuario.Correo,
            usuario.Proceso,
            usuario.Activo,
            rolActivo?.Id,
            rolActivo?.Nombre
        );
    }
}
