namespace PlataformaActas.Api.Modules.Roles.Models.Dto;

public sealed record ActualizarRolRequest(
    string Nombre,
    string? Descripcion
);
