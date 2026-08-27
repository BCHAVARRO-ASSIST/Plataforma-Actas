namespace PlataformaActas.Api.Modules.Usuarios.Models.Dto;

public sealed record UsuarioCreateRequest(
    string Nombre,
    string Apellido,
    string UsuarioRed,
    string Correo,
    string Proceso
);
