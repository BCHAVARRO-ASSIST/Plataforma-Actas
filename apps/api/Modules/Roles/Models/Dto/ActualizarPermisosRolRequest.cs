namespace PlataformaActas.Api.Modules.Roles.Models.Dto;

public sealed record ActualizarPermisosRolRequest(
    IReadOnlyList<int> PermisoIds
);
