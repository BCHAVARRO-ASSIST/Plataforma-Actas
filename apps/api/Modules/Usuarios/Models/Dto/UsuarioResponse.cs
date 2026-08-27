namespace PlataformaActas.Api.Modules.Usuarios.Models.Dto;

public sealed record UsuarioResponse(
    int Id,
    string Nombre,
    string Apellido,
    string UsuarioRed,
    string Correo,
    string Proceso,
    bool Activo,
    int? RolId,
    string? RolNombre
);
