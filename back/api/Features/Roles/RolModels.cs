namespace PlataformaActas.Api.Features.Roles;

public sealed record RolResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion
);

public sealed record PermisoResponse(
    int Id,
    string Nombre,
    string Modulo,
    string? Descripcion,
    bool Activo
);

public sealed record RolDetalleResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion,
    IReadOnlyList<PermisoResponse> Permisos
);

public sealed record CrearRolRequest(
    string Nombre,
    string? Descripcion
);

public sealed record ActualizarRolRequest(
    string Nombre,
    string? Descripcion
);

public sealed record ActualizarPermisosRolRequest(
    IReadOnlyList<int> PermisoIds
);