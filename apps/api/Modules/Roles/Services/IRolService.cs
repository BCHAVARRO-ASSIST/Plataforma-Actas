using PlataformaActas.Api.Modules.Roles.Models.Dto;

namespace PlataformaActas.Api.Modules.Roles.Services;

public enum EliminarRolResultado
{
    Eliminado,
    NoEncontrado,
    TieneUsuarios,
    AdministradorProtegido
}

public interface IRolService
{
    Task<IReadOnlyList<RolResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RolDetalleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RolResponse> CreateAsync(CrearRolRequest request, CancellationToken cancellationToken = default);
    Task<RolResponse?> UpdateAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default);
    Task<EliminarRolResultado> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermisoResponse>> GetPermisosAsync(CancellationToken cancellationToken = default);
    Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default);
}
