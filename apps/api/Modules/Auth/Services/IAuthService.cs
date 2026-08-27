using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

public interface IAuthService
{
    Task<LoginData?> AuthenticateAsync(string usuario, string password, CancellationToken cancellationToken = default);
}
