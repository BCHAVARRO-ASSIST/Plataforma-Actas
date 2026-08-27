namespace PlataformaActas.Api.Features.Usuarios;

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

public sealed record UsuarioCreateRequest(
    string Nombre,
    string Apellido,
    string UsuarioRed,
    string Correo,
    string Proceso
);

public sealed record UsuarioUpdateRequest(
    string Nombre,
    string Apellido,
    string UsuarioRed,
    string Correo,
    string Proceso,
    bool Activo
);

public sealed record UsuarioRolRequest(
    int RolId
);