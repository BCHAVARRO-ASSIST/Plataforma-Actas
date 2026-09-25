using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using PlataformaActas.Api.Infrastructure.Email;
using PlataformaActas.Api.Modules.Actas.Services;
using PlataformaActas.Api.Modules.Auth;
using PlataformaActas.Api.Modules.Auth.Services;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public class EmailSenderIsolationTests
{
    private static ActaEmailNotification CreateSampleNotification()
    {
        return new ActaEmailNotification(
            ToEmails: new List<string> { "destinatario@empresa.com" },
            CcEmails: new List<string> { "copia@empresa.com" },
            BccEmails: new List<string>(),
            DestinatarioNombre: "Juan Perez",
            NumeroActa: "P8-FR-003-ACTA-2026-0001",
            TipoActa: "Entrega",
            EquipoSerial: "ABC-12345",
            EquipoMarca: "Lenovo ThinkPad",
            PdfBytes: new byte[] { 1, 2, 3, 4 },
            FechaFirmaUtc: DateTime.UtcNow
        );
    }

    [Fact]
    public void SmtpEmailSenderService_CreateMimeMessage_UsesOfficialInstitutionalSender_AndSetsReplyToHeader()
    {
        // Arrange
        var notification = CreateSampleNotification();
        var officialSenderEmail = "soporte@assist.com.co";
        var officialSenderName = "Soporte - Assist Consultores";
        var smtpAuthUsername = "usuario.personal@assist.com.co";

        // Act
        var mimeMessage = SmtpEmailSenderService.CreateMimeMessage(
            notification,
            officialSenderEmail,
            officialSenderName);

        // Assert
        Assert.NotNull(mimeMessage);
        Assert.Single(mimeMessage.From);
        Assert.Single(mimeMessage.ReplyTo);

        var fromMailbox = mimeMessage.From.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", fromMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", fromMailbox.Name);

        var replyToMailbox = mimeMessage.ReplyTo.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", replyToMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", replyToMailbox.Name);

        // Validar estrictamente que el remitente NO sea la cuenta de autenticación personal / buzón SMTP
        Assert.NotEqual(smtpAuthUsername, fromMailbox.Address);
    }

    [Fact]
    public void SmtpEmailSenderService_CreateMimeMessage_DefaultsToOfficialInstitutionalSender_WhenSenderNullOrEmpty()
    {
        // Arrange
        var notification = CreateSampleNotification();

        // Act
        var mimeMessage = SmtpEmailSenderService.CreateMimeMessage(notification, string.Empty, string.Empty);

        // Assert
        Assert.NotNull(mimeMessage);
        Assert.Single(mimeMessage.From);
        Assert.Single(mimeMessage.ReplyTo);

        var fromMailbox = mimeMessage.From.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", fromMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", fromMailbox.Name);

        var replyToMailbox = mimeMessage.ReplyTo.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", replyToMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", replyToMailbox.Name);
    }

    [Fact]
    public void EmailService_CreatePasswordResetMessage_UsesOfficialInstitutionalSender_AndSetsReplyToList()
    {
        // Arrange
        var toEmail = "colaborador@empresa.com";
        var resetUrl = "https://actas.empresa.com/reset-password?token=abcdef";
        var officialSenderEmail = "soporte@assist.com.co";
        var officialSenderName = "Soporte - Assist Consultores";
        var smtpAuthUsername = "admin.personal@empresa.com";

        // Act
        var mailMessage = EmailService.CreatePasswordResetMessage(
            toEmail,
            resetUrl,
            officialSenderEmail,
            officialSenderName);

        // Assert
        Assert.NotNull(mailMessage);
        Assert.NotNull(mailMessage.From);
        Assert.Equal("soporte@assist.com.co", mailMessage.From.Address);
        Assert.Equal("Soporte - Assist Consultores", mailMessage.From.DisplayName);

        Assert.Single(mailMessage.ReplyToList);
        Assert.Equal("soporte@assist.com.co", mailMessage.ReplyToList.First().Address);

        // Validar aislamiento estricto de credenciales personales
        Assert.NotEqual(smtpAuthUsername, mailMessage.From.Address);
    }

    [Fact]
    public void EmailService_CreatePasswordResetMessage_DefaultsToOfficialInstitutionalSender_WhenSenderNullOrEmpty()
    {
        // Arrange
        var toEmail = "colaborador@empresa.com";
        var resetUrl = "https://actas.empresa.com/reset-password?token=abcdef";

        // Act
        var mailMessage = EmailService.CreatePasswordResetMessage(toEmail, resetUrl, string.Empty, string.Empty);

        // Assert
        Assert.NotNull(mailMessage);
        Assert.NotNull(mailMessage.From);
        Assert.Equal("soporte@assist.com.co", mailMessage.From.Address);
        Assert.Equal("Soporte - Assist Consultores", mailMessage.From.DisplayName);
        Assert.Single(mailMessage.ReplyToList);
        Assert.Equal("soporte@assist.com.co", mailMessage.ReplyToList.First().Address);
    }

    [Fact]
    public void ActaService_BuildRecipientsList_IncludesOnlyConsultorAndValidAdditionalEmails_ExcludesOperatorAndAdmins()
    {
        // Arrange
        var consultorEmail = "consultor.asignado@assist.com.co";
        var operatorCreatorEmail = "operador.registro@assist.com.co";
        var systemAdminEmail = "admin.sistema@assist.com.co";
        var additionalEmails = new List<string>
        {
            "notificaciones.equipo@cliente.com",
            operatorCreatorEmail,               // Intento de incluir al creador: debe ser eliminado
            systemAdminEmail,                   // Correo de administración: debe ser eliminado
            "correo-invalido-sin-arroba",        // Correo malformado: debe ser filtrado
            "  NOTIFICACIONES.EQUIPO@CLIENTE.COM  ", // Duplicado con espacios: debe ser dedup
            "valido2@empresa.com"
        };

        // Act
        var recipients = ActaService.BuildRecipientsList(
            consultorEmail: consultorEmail,
            correosAdicionales: additionalEmails,
            creatorEmail: operatorCreatorEmail,
            excludedEmails: new[] { systemAdminEmail });

        // Assert
        Assert.NotNull(recipients);
        Assert.Equal(3, recipients.Count);

        // 1. Debe incluir al consultor titular
        Assert.Contains(consultorEmail, recipients, StringComparer.OrdinalIgnoreCase);

        // 2. Debe incluir los correos adicionales válidos
        Assert.Contains("notificaciones.equipo@cliente.com", recipients, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("valido2@empresa.com", recipients, StringComparer.OrdinalIgnoreCase);

        // 3. NO debe incluir al operador / creador del acta
        Assert.DoesNotContain(operatorCreatorEmail, recipients, StringComparer.OrdinalIgnoreCase);

        // 4. NO debe incluir correos de administración predeterminados
        Assert.DoesNotContain(systemAdminEmail, recipients, StringComparer.OrdinalIgnoreCase);

        // 5. NO debe incluir formatos inválidos
        Assert.DoesNotContain("correo-invalido-sin-arroba", recipients, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActaService_BuildRecipientsList_WhenConsultorEmailIsNull_ReturnsOnlyValidAdditionalEmailsWithoutThrowing()
    {
        // Arrange
        string? nullConsultorEmail = null;
        var additionalEmails = new List<string>
        {
            "copia1@empresa.com",
            "copia2@empresa.com"
        };

        // Act
        var recipients = ActaService.BuildRecipientsList(
            consultorEmail: nullConsultorEmail,
            correosAdicionales: additionalEmails,
            creatorEmail: "creador@assist.com.co");

        // Assert
        Assert.NotNull(recipients);
        Assert.Equal(2, recipients.Count);
        Assert.Contains("copia1@empresa.com", recipients, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("copia2@empresa.com", recipients, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConfigurationBinding_SupportsBothEmailSettingsAndSmtp_WithSenderEmailSeparatedFromUsername()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "EmailSettings:Server", "smtp.gmail.com" },
            { "EmailSettings:Port", "587" },
            { "EmailSettings:SenderEmail", "soporte@assist.com.co" },
            { "EmailSettings:SenderName", "Soporte - Assist Consultores" },
            { "EmailSettings:Username", "sistema.autenticacion@assist.com.co" },
            { "EmailSettings:Password", "SuperSecretAppPassword123!" },
            { "EmailSettings:EnableSsl", "true" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.Configure<SmtpOptions>(options =>
        {
            configuration.GetSection(SmtpOptions.SectionName).Bind(options);
            configuration.GetSection(SmtpOptions.EmailSettingsSectionName).Bind(options);
        });
        services.Configure<EmailSettings>(options =>
        {
            configuration.GetSection(SmtpOptions.SectionName).Bind(options);
            configuration.GetSection(EmailSettings.SectionName).Bind(options);
        });

        var provider = services.BuildServiceProvider();
        var smtpOptions = provider.GetRequiredService<IOptions<SmtpOptions>>().Value;
        var emailSettings = provider.GetRequiredService<IOptions<EmailSettings>>().Value;

        // Assert SmtpOptions
        Assert.Equal("smtp.gmail.com", smtpOptions.Host);
        Assert.Equal("smtp.gmail.com", smtpOptions.Server);
        Assert.Equal(587, smtpOptions.Port);
        Assert.Equal("soporte@assist.com.co", smtpOptions.SenderEmail);
        Assert.Equal("soporte@assist.com.co", smtpOptions.FromEmail);
        Assert.Equal("Soporte - Assist Consultores", smtpOptions.SenderName);
        Assert.Equal("Soporte - Assist Consultores", smtpOptions.FromName);
        Assert.Equal("sistema.autenticacion@assist.com.co", smtpOptions.Username);
        Assert.Equal("sistema.autenticacion@assist.com.co", smtpOptions.UserName);
        Assert.True(smtpOptions.EnableSsl);

        // El remitente debe estar completamente diferenciado de las credenciales de autenticación
        Assert.NotEqual(smtpOptions.SenderEmail, smtpOptions.Username);

        // Assert EmailSettings
        Assert.Equal("smtp.gmail.com", emailSettings.Server);
        Assert.Equal(587, emailSettings.Port);
        Assert.Equal("soporte@assist.com.co", emailSettings.SenderEmail);
        Assert.Equal("Soporte - Assist Consultores", emailSettings.SenderName);
        Assert.Equal("sistema.autenticacion@assist.com.co", emailSettings.Username);
        Assert.NotEqual(emailSettings.SenderEmail, emailSettings.Username);
    }

    [Fact]
    public void GmailEmailSenderService_CreateMimeMessage_UsesOfficialInstitutionalSender_AndSetsReplyToHeader()
    {
        // Arrange
        var notification = CreateSampleNotification();
        var officialSenderEmail = "soporte@assist.com.co";
        var officialSenderName = "Soporte - Assist Consultores";

        // Act
        var mimeMessage = GmailEmailSenderService.CreateMimeMessage(
            notification,
            officialSenderEmail,
            officialSenderName);

        // Assert
        Assert.NotNull(mimeMessage);
        Assert.Single(mimeMessage.From);
        Assert.Single(mimeMessage.ReplyTo);

        var fromMailbox = mimeMessage.From.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", fromMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", fromMailbox.Name);

        var replyToMailbox = mimeMessage.ReplyTo.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", replyToMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", replyToMailbox.Name);
    }

    [Fact]
    public void GmailEmailSenderService_CreateMimeMessage_DefaultsToOfficialInstitutionalSender_WhenSenderNullOrEmpty()
    {
        // Arrange
        var notification = CreateSampleNotification();

        // Act
        var mimeMessage = GmailEmailSenderService.CreateMimeMessage(notification, string.Empty, string.Empty);

        // Assert
        Assert.NotNull(mimeMessage);
        Assert.Single(mimeMessage.From);
        Assert.Single(mimeMessage.ReplyTo);

        var fromMailbox = mimeMessage.From.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", fromMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", fromMailbox.Name);

        var replyToMailbox = mimeMessage.ReplyTo.Mailboxes.First();
        Assert.Equal("soporte@assist.com.co", replyToMailbox.Address);
        Assert.Equal("Soporte - Assist Consultores", replyToMailbox.Name);
    }

    [Fact]
    public void GoogleIntegrationOptions_DefaultsMatchDomainWideDelegationAndSecretsPath()
    {
        // Arrange
        var options = new PlataformaActas.Api.Modules.Integrations.Google.GoogleIntegrationOptions();

        // Assert
        Assert.Equal("Secrets/google-credentials.json", options.ServiceAccountKeyPath);
        Assert.Equal("soporte@assist.com.co", options.AdminImpersonationEmail);
        Assert.Equal("soporte@assist.com.co", options.GmailImpersonationEmail);
        Assert.Equal("104519674085578864792", options.ClientId);
        Assert.True(options.Enabled);
    }
}
