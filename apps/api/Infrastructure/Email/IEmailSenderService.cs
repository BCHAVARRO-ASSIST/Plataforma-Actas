namespace PlataformaActas.Api.Infrastructure.Email;

public interface IEmailSenderService
{
    Task SendActaNotificationAsync(ActaEmailNotification notification, CancellationToken cancellationToken = default);
}
