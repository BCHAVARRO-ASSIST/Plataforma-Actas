using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data;

public interface IRolRepository
{
    Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Rol?> GetByIdWithPermisosAsync(int id, CancellationToken cancellationToken = default);
    Task<Rol?> GetByNameAsync(string nombre, CancellationToken cancellationToken = default);
    Task<Rol> AddAsync(Rol rol, CancellationToken cancellationToken = default);
    Task UpdateAsync(Rol rol, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasAssignedUsersAsync(int rolId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permiso>> GetAllPermisosAsync(CancellationToken cancellationToken = default);
    Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
