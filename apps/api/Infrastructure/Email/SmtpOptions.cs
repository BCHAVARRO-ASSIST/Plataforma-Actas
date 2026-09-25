namespace PlataformaActas.Api.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = "soporte@assist.com.co";
    public string SenderName { get; set; } = "Soporte - Assist Consultores";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public const string EmailSettingsSectionName = "EmailSettings";

    public string Host { get; set; } = "localhost";
    public string Server
    {
        get => Host;
        set => Host = value;
    }

    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;

    public string UserName { get; set; } = string.Empty;
    public string Username
    {
        get => UserName;
        set => UserName = value;
    }

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = "soporte@assist.com.co";
    public string SenderEmail
    {
        get => FromEmail;
        set => FromEmail = value;
    }

    public string FromName { get; set; } = "Soporte - Assist Consultores";
    public string SenderName
    {
        get => FromName;
        set => FromName = value;
    }

    public List<string> SoporteNotificationEmails { get; set; } = new();
    public List<string> ApsNotificationEmails { get; set; } = new();
}
