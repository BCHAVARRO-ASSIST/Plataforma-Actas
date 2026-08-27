namespace PlataformaActas.Api.Modules.Roles.Models.Dto;

public sealed record CrearRolRequest(
    string Nombre,
    string? Descripcion
);
