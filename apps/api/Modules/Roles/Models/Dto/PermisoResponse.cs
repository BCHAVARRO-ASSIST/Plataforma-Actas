namespace PlataformaActas.Api.Modules.Roles.Models.Dto;

public sealed record PermisoResponse(
    int Id,
    string Nombre,
    string Modulo,
    string? Descripcion,
    bool Activo
);
