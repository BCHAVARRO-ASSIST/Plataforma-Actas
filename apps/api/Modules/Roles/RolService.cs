using PlataformaActas.Api.Modules.Roles.Data;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Roles.Models.Dto;
using PlataformaActas.Api.Modules.Roles.Models.Mappers;

namespace PlataformaActas.Api.Modules.Roles.Services;

public sealed class RolService : IRolService
{
    private readonly IRolRepository _repository;

    public RolService(IRolRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RolResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _repository.GetAllAsync(cancellationToken);
        return roles.Select(r => r.ToResponse()).ToList();
    }

    public async Task<RolDetalleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _repository.GetByIdAsync(id, cancellationToken);
        return rol?.ToDetalleResponse();
    }

    public async Task<RolResponse> CreateAsync(CrearRolRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Nombre.Trim();
        if (await _repository.ExistsByNameAsync(trimmedName, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe un rol con el nombre '{trimmedName}'.");
        }

        var rol = new Rol
        {
            Nombre = trimmedName,
            Descripcion = request.Descripcion?.Trim() ?? string.Empty,
            Activo = true
        };

        var created = await _repository.CreateAsync(rol, cancellationToken);
        return created.ToResponse();
    }

    public async Task<RolResponse?> UpdateAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken = default)
    {
        var rol = await _repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return null;
        }

        var trimmedName = request.Nombre.Trim();
        if (await _repository.ExistsByNameAsync(trimmedName, id, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe otro rol con el nombre '{trimmedName}'.");
        }

        rol.Nombre = trimmedName;
        rol.Descripcion = request.Descripcion?.Trim() ?? string.Empty;

        var updated = await _repository.UpdateAsync(rol, cancellationToken);
        return updated.ToResponse();
    }

    public async Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        var rol = await _repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return false;
        }

        rol.Activo = activo;
        await _repository.UpdateAsync(rol, cancellationToken);
        return true;
    }

    public async Task<EliminarRolResultado> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return EliminarRolResultado.NoEncontrado;
        }

        if (rol.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
        {
            return EliminarRolResultado.AdministradorProtegido;
        }

        if (await _repository.HasAssignedUsersAsync(id, cancellationToken))
        {
            return EliminarRolResultado.TieneUsuarios;
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted ? EliminarRolResultado.Eliminado : EliminarRolResultado.NoEncontrado;
    }

    public async Task<IReadOnlyList<PermisoResponse>> GetPermisosAsync(CancellationToken cancellationToken = default)
    {
        var permisos = await _repository.GetAllPermisosAsync(cancellationToken);
        return permisos.Select(p => p.ToResponse()).ToList();
    }

    public async Task<bool> SetPermisosAsync(int id, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        return await _repository.SetPermisosAsync(id, permisoIds, cancellationToken);
    }
}
