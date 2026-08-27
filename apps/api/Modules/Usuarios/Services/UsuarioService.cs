using PlataformaActas.Api.Modules.Usuarios.Data;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;
using PlataformaActas.Api.Modules.Usuarios.Models.Dto;
using PlataformaActas.Api.Modules.Usuarios.Models.Mappers;

namespace PlataformaActas.Api.Modules.Usuarios.Services;

public sealed class UsuarioService : IUsuarioService
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
