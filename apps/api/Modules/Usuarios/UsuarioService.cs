using PlataformaActas.Api.Modules.Usuarios.Data;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Models.Dto;
using PlataformaActas.Api.Modules.Usuarios.Models.Mappers;

namespace PlataformaActas.Api.Modules.Usuarios.Services;

public sealed class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _repository.GetAllAsync(cancellationToken);
        return usuarios.Select(u => u.ToResponse()).ToList();
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.GetByIdAsync(id, cancellationToken);
        return usuario?.ToResponse();
    }

    public async Task<UsuarioResponse> CreateAsync(UsuarioCreateRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedUserRed = request.UsuarioRed.Trim();
        var trimmedEmail = request.Correo.Trim();

        var existingUser = await _repository.GetByUsuarioRedAsync(trimmedUserRed, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Ya existe un usuario con el identificador '{trimmedUserRed}'.");
        }

        var existingEmail = await _repository.GetByCorreoAsync(trimmedEmail, cancellationToken);
        if (existingEmail != null)
        {
            throw new InvalidOperationException($"Ya existe un usuario con el correo '{trimmedEmail}'.");
        }

        var defaultPassword = BCrypt.Net.BCrypt.HashPassword("Actas2026*");

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            UsuarioRed = trimmedUserRed,
            Correo = trimmedEmail,
            PasswordHash = defaultPassword,
            Proceso = request.Proceso.Trim(),
            Activo = true
        };

        var created = await _repository.AddAsync(usuario, cancellationToken);
        var reloaded = await _repository.GetByIdAsync(created.Id, cancellationToken);
        return (reloaded ?? created).ToResponse();
    }

    public async Task<UsuarioResponse?> UpdateAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.GetByIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            return null;
        }

        var trimmedUserRed = request.UsuarioRed.Trim();
        var trimmedEmail = request.Correo.Trim();

        var existingUser = await _repository.GetByUsuarioRedAsync(trimmedUserRed, cancellationToken);
        if (existingUser != null && existingUser.Id != id)
        {
            throw new InvalidOperationException($"Ya existe otro usuario con el identificador '{trimmedUserRed}'.");
        }

        var existingEmail = await _repository.GetByCorreoAsync(trimmedEmail, cancellationToken);
        if (existingEmail != null && existingEmail.Id != id)
        {
            throw new InvalidOperationException($"Ya existe otro usuario con el correo '{trimmedEmail}'.");
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.UsuarioRed = trimmedUserRed;
        usuario.Correo = trimmedEmail;
        usuario.Proceso = request.Proceso.Trim();
        usuario.Activo = request.Activo;

        await _repository.UpdateAsync(usuario, cancellationToken);

        var reloaded = await _repository.GetByIdAsync(usuario.Id, cancellationToken);
        return (reloaded ?? usuario).ToResponse();
    }

    public async Task<bool> SetActivoAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        return await _repository.SetActiveAsync(id, activo, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.SetActiveAsync(id, false, cancellationToken);
    }

    public async Task<UsuarioResponse?> AssignRoleAsync(int id, int rolId, CancellationToken cancellationToken = default)
    {
        var ok = await _repository.AssignRoleAsync(id, rolId, cancellationToken);
        if (!ok)
        {
            return null;
        }

        var reloaded = await _repository.GetByIdAsync(id, cancellationToken);
        return reloaded?.ToResponse();
    }
}
