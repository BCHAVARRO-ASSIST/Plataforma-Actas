using PlataformaActas.Api.Modules.Usuarios.Data;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Models.Dto;
using PlataformaActas.Api.Modules.Usuarios.Models.Mappers;

namespace PlataformaActas.Api.Modules.Usuarios.Services;

public sealed class UsuarioService(IUsuarioRepository repository) : IUsuarioService
{

    public async Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await repository.GetAllAsync(cancellationToken);
        return usuarios.Select(u => u.ToResponse()).ToList();
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(id, cancellationToken);
        return usuario?.ToResponse();
    }

    public async Task<UsuarioResponse> CreateAsync(UsuarioCreateRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedUserRed = request.UsuarioRed.Trim();
        var trimmedEmail = request.Correo.Trim();

        var existingUser = await repository.GetByUsuarioRedAsync(trimmedUserRed, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Ya existe un usuario con el identificador '{trimmedUserRed}'.");
        }

        var existingEmail = await repository.GetByCorreoAsync(trimmedEmail, cancellationToken);
        if (existingEmail != null)
        {
            throw new InvalidOperationException($"Ya existe un usuario con el correo '{trimmedEmail}'.");
        }

        var rawPassword = !string.IsNullOrWhiteSpace(request.Password) ? request.Password.Trim() : "Actas2026*";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            UsuarioRed = trimmedUserRed,
            Correo = trimmedEmail,
            PasswordHash = passwordHash,
            Proceso = request.Proceso.Trim(),
            Activo = true
        };

        var created = await repository.AddAsync(usuario, cancellationToken);

        if (request.RolId.HasValue && request.RolId.Value > 0)
        {
            await repository.AssignRoleAsync(created.Id, request.RolId.Value, cancellationToken);
        }

        var reloaded = await repository.GetByIdAsync(created.Id, cancellationToken);
        return (reloaded ?? created).ToResponse();
    }

    public async Task<UsuarioResponse?> UpdateAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            return null;
        }

        var trimmedUserRed = request.UsuarioRed.Trim();
        var trimmedEmail = request.Correo.Trim();

        var existingUser = await repository.GetByUsuarioRedAsync(trimmedUserRed, cancellationToken);
        if (existingUser != null && existingUser.Id != id)
        {
            throw new InvalidOperationException($"Ya existe otro usuario con el identificador '{trimmedUserRed}'.");
        }

        var existingEmail = await repository.GetByCorreoAsync(trimmedEmail, cancellationToken);
        if (existingEmail != null && existingEmail.Id != id)
        {
            throw new InvalidOperationException($"Ya existe otro usuario con el correo '{trimmedEmail}'.");
        }

        if (usuario.UsuarioRed.Equals("admin", StringComparison.OrdinalIgnoreCase) && !request.Activo)
        {
            throw new InvalidOperationException("El usuario administrador principal 'admin' no puede ser desactivado.");
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.UsuarioRed = trimmedUserRed;
        usuario.Correo = trimmedEmail;
        usuario.Proceso = request.Proceso.Trim();
        usuario.Activo = request.Activo;

        await repository.UpdateAsync(usuario, cancellationToken);

        var reloaded = await repository.GetByIdAsync(usuario.Id, cancellationToken);
        return (reloaded ?? usuario).ToResponse();
    }

    public async Task<bool> SetActivoAsync(int id, bool activo, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(id, cancellationToken);
        if (usuario != null && usuario.UsuarioRed.Equals("admin", StringComparison.OrdinalIgnoreCase) && !activo)
        {
            throw new InvalidOperationException("El usuario administrador principal 'admin' no puede ser desactivado para evitar el bloqueo del sistema.");
        }

        return await repository.SetActiveAsync(id, activo, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await SetActivoAsync(id, false, cancellationToken);
    }

    public async Task<UsuarioResponse?> AssignRoleAsync(int id, int rolId, CancellationToken cancellationToken = default)
    {
        var ok = await repository.AssignRoleAsync(id, rolId, cancellationToken);
        if (!ok)
        {
            return null;
        }

        var reloaded = await repository.GetByIdAsync(id, cancellationToken);
        return reloaded?.ToResponse();
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            return false;
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await repository.UpdateAsync(usuario, cancellationToken);
        return true;
    }
}
