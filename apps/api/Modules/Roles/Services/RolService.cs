using PlataformaActas.Api.Modules.Roles.Data;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Roles.Models.Dto;
using PlataformaActas.Api.Modules.Roles.Models.Mappers;

namespace PlataformaActas.Api.Modules.Roles.Services;

public sealed class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;

    public RolService(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
    }

    public async Task<IReadOnlyList<RolResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _rolRepository.GetAllAsync(cancellationToken);
        return roles.Select(r => r.ToResponse()).ToList();
    }

    public async Task<RolDetalleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _rolRepository.GetByIdWithPermisosAsync(id, cancellationToken);
        return rol?.ToDetalleResponse();
    }

    public async Task<RolResponse> CreateAsync(CrearRolRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Nombre.Trim();
        var existing = await _rolRepository.GetByNameAsync(trimmedName, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Ya existe un rol con el nombre '{trimmedName}'.");
        }

        var rol = new Rol
        {
            Nombre = trimmedName,
            Descripcion = request.Descripcion?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var created = await _rolRepository.AddAsync(rol, cancellationToken);
        return created.ToResponse();
    }

    public async Task<RolResponse?> UpdateAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken = default)
    {
        var rol = await _rolRepository.GetByIdAsync(id, cancellationToken);
        if (rol is null)
        {
            return null;
        }

        var trimmedName = request.Nombre.Trim();
        var existingWithName = await _rolRepository.GetByNameAsync(trimmedName, cancellationToken);
        if (existingWithName is not null && existingWithName.Id != id)
        {
            throw new InvalidOperationException($"Ya existe otro rol con el nombre '{trimmedName}'.");
        }

        rol.Nombre = trimmedName;
        rol.Descripcion = request.Descripcion?.Trim();

        await _rolRepository.UpdateAsync(rol, cancellationToken);
        return rol.ToResponse();
    }

    public async Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        var rol = await _rolRepository.GetByIdAsync(id, cancellationToken);
        if (rol is null)
        {
            return false;
        }

        rol.Activo = activo;
        await _rolRepository.UpdateAsync(rol, cancellationToken);
        return true;
    }

    public async Task<EliminarRolResultado> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _rolRepository.GetByIdAsync(id, cancellationToken);
        if (rol is null)
        {
            return EliminarRolResultado.NoEncontrado;
        }

        if (string.Equals(rol.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase))
        {
            return EliminarRolResultado.AdministradorProtegido;
        }

        var hasUsers = await _rolRepository.HasAssignedUsersAsync(id, cancellationToken);
        if (hasUsers)
        {
            return EliminarRolResultado.TieneUsuarios;
        }

        var deleted = await _rolRepository.DeleteAsync(id, cancellationToken);
        return deleted ? EliminarRolResultado.Eliminado : EliminarRolResultado.NoEncontrado;
    }

    public async Task<IReadOnlyList<PermisoResponse>> GetPermisosAsync(CancellationToken cancellationToken = default)
    {
        var permisos = await _rolRepository.GetAllPermisosAsync(cancellationToken);
        return permisos.Select(p => p.ToResponse()).ToList();
    }

    public async Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        return await _rolRepository.SetPermisosAsync(rolId, permisoIds, cancellationToken);
    }
}
