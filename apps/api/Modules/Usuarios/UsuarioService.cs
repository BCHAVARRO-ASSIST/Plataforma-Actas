using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Roles.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Models.Dto;
using PlataformaActas.Api.Modules.Usuarios.Models.Mappers;

namespace PlataformaActas.Api.Modules.Usuarios.Data
{
    public interface IUsuarioRepository
    {
        Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Usuario?> GetByUsuarioRedAsync(string usuarioRed, CancellationToken cancellationToken = default);
        Task<Usuario?> GetByCorreoAsync(string correo, CancellationToken cancellationToken = default);
        Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default);
        Task<bool> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default);
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly PlataformaActasDbContext _dbContext;

        public UsuarioRepository(PlataformaActasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .ToListAsync(cancellationToken);
        }

        public async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<Usuario?> GetByUsuarioRedAsync(string usuarioRed, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioRed.ToLower() == usuarioRed.ToLower(), cancellationToken);
        }

        public async Task<Usuario?> GetByCorreoAsync(string correo, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower(), cancellationToken);
        }

        public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _dbContext.Usuarios.Add(usuario);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return usuario;
        }

        public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _dbContext.Usuarios.Update(usuario);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SetActiveAsync(int id, bool activo, CancellationToken cancellationToken = default)
        {
            var usuario = await _dbContext.Usuarios.FindAsync(new object[] { id }, cancellationToken);
            if (usuario is null)
            {
                return false;
            }

            usuario.Activo = activo;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default)
        {
            var usuario = await _dbContext.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

            if (usuario is null)
            {
                return false;
            }

            var rolExiste = await _dbContext.Roles
                .AnyAsync(r => r.Id == rolId && r.Activo, cancellationToken);

            if (!rolExiste)
            {
                throw new InvalidOperationException($"El rol con ID '{rolId}' no existe o se encuentra inactivo.");
            }

            _dbContext.UsuarioRoles.RemoveRange(usuario.UsuarioRoles);

            usuario.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuarioId,
                RolId = rolId,
                FechaAsignacion = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

namespace PlataformaActas.Api.Modules.Usuarios.Services
{
    using PlataformaActas.Api.Modules.Usuarios.Data;

    public interface IUsuarioService
    {
        Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UsuarioResponse> CreateAsync(UsuarioCreateRequest request, CancellationToken cancellationToken = default);
        Task<UsuarioResponse?> UpdateAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<UsuarioResponse?> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var usuarios = await _usuarioRepository.GetAllAsync(cancellationToken);
            return usuarios.Select(u => u.ToResponse()).ToList();
        }

        public async Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);
            return usuario?.ToResponse();
        }

        public async Task<UsuarioResponse> CreateAsync(UsuarioCreateRequest request, CancellationToken cancellationToken = default)
        {
            var usuarioRed = request.UsuarioRed.Trim();
            var correo = request.Correo.Trim();

            var existingByRed = await _usuarioRepository.GetByUsuarioRedAsync(usuarioRed, cancellationToken);
            if (existingByRed is not null)
            {
                throw new InvalidOperationException($"Ya existe un usuario con el usuario de red '{usuarioRed}'.");
            }

            var existingByCorreo = await _usuarioRepository.GetByCorreoAsync(correo, cancellationToken);
            if (existingByCorreo is not null)
            {
                throw new InvalidOperationException($"Ya existe un usuario con el correo '{correo}'.");
            }

            var usuario = new Usuario
            {
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                UsuarioRed = usuarioRed,
                Correo = correo,
                Proceso = request.Proceso.Trim(),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            var created = await _usuarioRepository.AddAsync(usuario, cancellationToken);
            return created.ToResponse();
        }

        public async Task<UsuarioResponse?> UpdateAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);
            if (usuario is null)
            {
                return null;
            }

            var usuarioRed = request.UsuarioRed.Trim();
            var correo = request.Correo.Trim();

            var existingByRed = await _usuarioRepository.GetByUsuarioRedAsync(usuarioRed, cancellationToken);
            if (existingByRed is not null && existingByRed.Id != id)
            {
                throw new InvalidOperationException($"Ya existe otro usuario con el usuario de red '{usuarioRed}'.");
            }

            var existingByCorreo = await _usuarioRepository.GetByCorreoAsync(correo, cancellationToken);
            if (existingByCorreo is not null && existingByCorreo.Id != id)
            {
                throw new InvalidOperationException($"Ya existe otro usuario con el correo '{correo}'.");
            }

            usuario.Nombre = request.Nombre.Trim();
            usuario.Apellido = request.Apellido.Trim();
            usuario.UsuarioRed = usuarioRed;
            usuario.Correo = correo;
            usuario.Proceso = request.Proceso.Trim();
            usuario.Activo = request.Activo;

            await _usuarioRepository.UpdateAsync(usuario, cancellationToken);
            return usuario.ToResponse();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.SetActiveAsync(id, false, cancellationToken);
        }

        public async Task<UsuarioResponse?> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default)
        {
            var ok = await _usuarioRepository.AssignRoleAsync(usuarioId, rolId, cancellationToken);
            if (!ok)
            {
                return null;
            }

            return await GetByIdAsync(usuarioId, cancellationToken);
        }
    }
}
