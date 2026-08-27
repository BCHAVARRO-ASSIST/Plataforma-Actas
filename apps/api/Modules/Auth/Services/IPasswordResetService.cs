using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

public interface IPasswordResetService
{
    Task<PasswordResetRequestResult> CreateResetRequestAsync(string identifier, CancellationToken cancellationToken = default);
    Task<PasswordResetResult> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}
