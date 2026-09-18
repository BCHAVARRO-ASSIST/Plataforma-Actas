using Microsoft.AspNetCore.Http;

namespace PlataformaActas.Api.Modules.Integrations.Google;

public interface IGoogleAuthService
{
    Task<IResult> AuthenticateWithGoogleAsync(string? idToken, CancellationToken cancellationToken = default);
}
