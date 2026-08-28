using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

public interface IJwtTokenService
{
    string GenerateToken(
        UsuarioAuthResponse usuario,
        RolAuthResponse? rol,
        IReadOnlyList<string> permisos);
}
