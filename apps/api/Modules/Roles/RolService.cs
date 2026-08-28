using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Roles.Models.Dto;
using PlataformaActas.Api.Modules.Roles.Models.Mappers;

namespace PlataformaActas.Api.Modules.Roles.Data
{
    public interface IRolRepository
    {
        Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<Rol> CreateAsync(Rol rol, CancellationToken cancellationToken = default);
        Task<Rol> UpdateAsync(Rol rol, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasAssignedUsersAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Permiso>> GetAllPermisosAsync(CancellationToken cancellationToken = default);
        Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default);
    }

    public class RolRepository : IRolRepository
    {
        private readonly PlataformaActasDbContext _dbContext;

        public RolRepository(PlataformaActasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Roles
                .AsNoTracking()
                .Where(r => r.Nombre.ToLower() == nombre.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<Rol> CreateAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            _dbContext.Roles.Add(rol);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return rol;
        }

        public async Task<Rol> UpdateAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            _dbContext.Roles.Update(rol);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return rol;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var rol = await _dbContext.Roles.FindAsync(new object[] { id }, cancellationToken);
            if (rol is null)
            {
                return false;
            }

            _dbContext.Roles.Remove(rol);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HasAssignedUsersAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UsuarioRoles
                .AsNoTracking()
                .AnyAsync(ur => ur.RolId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Permiso>> GetAllPermisosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Permisos
                .AsNoTracking()
                .OrderBy(p => p.Modulo)
                .ThenBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> SetPermisosAsync(int rolId, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
        {
            var rol = await _dbContext.Roles
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);

            if (rol is null)
            {
                return false;
            }

            _dbContext.RolPermisos.RemoveRange(rol.RolPermisos);

            var validPermisoIds = await _dbContext.Permisos
                .Where(p => permisoIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            foreach (var permisoId in validPermisoIds)
            {
                rol.RolPermisos.Add(new RolPermiso
                {
                    RolId = rolId,
                    PermisoId = permisoId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

namespace PlataformaActas.Api.Modules.Roles.Services
{
    using PlataformaActas.Api.Modules.Roles.Data;

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

    public class RolService : IRolService
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
            var rol = await _rolRepository.GetByIdAsync(id, cancellationToken);
            return rol?.ToDetalleResponse();
        }

        public async Task<RolResponse> CreateAsync(CrearRolRequest request, CancellationToken cancellationToken = default)
        {
            var nombreNormalizado = request.Nombre.Trim();

            if (await _rolRepository.ExistsByNameAsync(nombreNormalizado, null, cancellationToken))
            {
                throw new InvalidOperationException($"Ya existe un rol con el nombre '{nombreNormalizado}'.");
            }

            var rol = new Rol
            {
                Nombre = nombreNormalizado,
                Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _rolRepository.CreateAsync(rol, cancellationToken);
            return creado.ToResponse();
        }

        public async Task<RolResponse?> UpdateAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken = default)
        {
            var rol = await _rolRepository.GetByIdAsync(id, cancellationToken);
            if (rol is null)
            {
                return null;
            }

            var nombreNormalizado = request.Nombre.Trim();

            if (await _rolRepository.ExistsByNameAsync(nombreNormalizado, id, cancellationToken))
            {
                throw new InvalidOperationException($"Ya existe otro rol con el nombre '{nombreNormalizado}'.");
            }

            rol.Nombre = nombreNormalizado;
            rol.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();

            var actualizado = await _rolRepository.UpdateAsync(rol, cancellationToken);
            return actualizado.ToResponse();
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
}
