using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Usuarios.Data;

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
