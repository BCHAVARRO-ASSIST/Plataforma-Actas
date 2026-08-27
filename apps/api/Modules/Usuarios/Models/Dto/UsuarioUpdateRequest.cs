namespace PlataformaActas.Api.Modules.Usuarios.Models.Dto;

public sealed record UsuarioUpdateRequest(
    string Nombre,
    string Apellido,
    string UsuarioRed,
    string Correo,
    string Proceso,
    bool Activo
);
