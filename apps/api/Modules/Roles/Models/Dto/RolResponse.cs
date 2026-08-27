namespace PlataformaActas.Api.Modules.Roles.Models.Dto;

public sealed record RolResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion
);
