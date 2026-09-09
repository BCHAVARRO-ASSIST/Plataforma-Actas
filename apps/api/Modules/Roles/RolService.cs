using PlataformaActas.Api.Modules.Roles.Data;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Roles.Models.Dto;
using PlataformaActas.Api.Modules.Roles.Models.Mappers;

namespace PlataformaActas.Api.Modules.Roles.Services;

public sealed class RolService(IRolRepository repository) : IRolService
{

    public async Task<IReadOnlyList<RolResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await repository.GetAllAsync(cancellationToken);
        return roles.Select(r => r.ToResponse()).ToList();
    }

    public async Task<RolDetalleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await repository.GetByIdAsync(id, cancellationToken);
        return rol?.ToDetalleResponse();
    }

    public async Task<RolResponse> CreateAsync(CrearRolRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Nombre.Trim();
        if (await repository.ExistsByNameAsync(trimmedName, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe un rol con el nombre '{trimmedName}'.");
        }

        var rol = new Rol
        {
            Nombre = trimmedName,
            Descripcion = request.Descripcion?.Trim() ?? string.Empty,
            Activo = true
        };

        var created = await repository.CreateAsync(rol, cancellationToken);
        return created.ToResponse();
    }

    public async Task<RolResponse?> UpdateAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken = default)
    {
        var rol = await repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return null;
        }

        var trimmedName = request.Nombre.Trim();
        if (await repository.ExistsByNameAsync(trimmedName, id, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe otro rol con el nombre '{trimmedName}'.");
        }

        rol.Nombre = trimmedName;
        rol.Descripcion = request.Descripcion?.Trim() ?? string.Empty;

        var updated = await repository.UpdateAsync(rol, cancellationToken);
        return updated.ToResponse();
    }

    public async Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        var rol = await repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return false;
        }

        if (rol.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase) && !activo)
        {
            throw new InvalidOperationException("El rol 'Administrador' no puede ser desactivado por ser el rol raíz del sistema.");
        }

        rol.Activo = activo;
        await repository.UpdateAsync(rol, cancellationToken);
        return true;
    }

    public async Task<EliminarRolResultado> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return EliminarRolResultado.NoEncontrado;
        }

        if (rol.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
        {
            return EliminarRolResultado.AdministradorProtegido;
        }

        if (await repository.HasAssignedUsersAsync(id, cancellationToken))
        {
            return EliminarRolResultado.TieneUsuarios;
        }

        var deleted = await repository.DeleteAsync(id, cancellationToken);
        return deleted ? EliminarRolResultado.Eliminado : EliminarRolResultado.NoEncontrado;
    }

    public async Task<IReadOnlyList<PermisoResponse>> GetPermisosAsync(CancellationToken cancellationToken = default)
    {
        var permisos = await repository.GetAllPermisosAsync(cancellationToken);
        return permisos.Select(p => p.ToResponse()).ToList();
    }

    public async Task<bool> SetPermisosAsync(int id, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        var rol = await repository.GetByIdAsync(id, cancellationToken);
        if (rol == null)
        {
            return false;
        }

        if (rol.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
        {
            if (permisoIds == null || permisoIds.Count == 0)
            {
                throw new InvalidOperationException("No es posible revocar todos los permisos del rol 'Administrador' para evitar el bloqueo del sistema.");
            }

            var allPermisos = await repository.GetAllPermisosAsync(cancellationToken);
            var criticalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ROLES_CONSULTAR",
                "ROLES_EDITAR",
                "USUARIOS_CONSULTAR",
                "DASHBOARD_CONSULTAR"
            };

            var criticalIds = allPermisos.Where(p => criticalNames.Contains(p.Nombre)).Select(p => p.Id).ToList();
            var missingCritical = criticalIds.Where(cId => !permisoIds.Contains(cId)).ToList();

            if (missingCritical.Count > 0)
            {
                throw new InvalidOperationException("El rol 'Administrador' debe conservar obligatoriamente los permisos esenciales de acceso y gestión (Dashboard, Roles y Usuarios).");
            }
        }

        return await repository.SetPermisosAsync(id, permisoIds, cancellationToken);
    }
}
