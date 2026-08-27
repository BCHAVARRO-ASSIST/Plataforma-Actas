using PlataformaActas.Api.Modules.Usuarios.Models.Dto;

namespace PlataformaActas.Api.Modules.Usuarios.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UsuarioResponse> CreateAsync(UsuarioCreateRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioResponse?> UpdateAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<UsuarioResponse?> AssignRoleAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default);
}
