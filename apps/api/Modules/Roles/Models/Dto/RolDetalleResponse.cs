namespace PlataformaActas.Api.Modules.Roles.Models.Dto;

public sealed record RolDetalleResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion,
    IReadOnlyList<PermisoResponse> Permisos
);
