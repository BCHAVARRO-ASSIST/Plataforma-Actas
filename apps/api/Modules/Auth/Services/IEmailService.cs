namespace PlataformaActas.Api.Modules.Auth.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}
