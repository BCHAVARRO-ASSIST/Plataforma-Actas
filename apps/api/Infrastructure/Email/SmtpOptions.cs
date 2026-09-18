namespace PlataformaActas.Api.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "soporte@assist.com.co";
    public string FromName { get; set; } = "Assist Consultores de Sistemas - Soporte IT";

    public List<string> SoporteNotificationEmails { get; set; } = new();
    public List<string> ApsNotificationEmails { get; set; } = new();
}
