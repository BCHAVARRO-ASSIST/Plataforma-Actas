namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed class GoogleIntegrationOptions
{
    public const string SectionName = "GoogleIntegration";

    public string ServiceAccountKeyPath { get; set; } = "Secrets/google-credentials.json";
    public string AdminImpersonationEmail { get; set; } = "soporte@assist.com.co";
    public string GmailImpersonationEmail { get; set; } = "soporte@assist.com.co";
    public string Domain { get; set; } = "assist.com.co";
    public string DriveFileId { get; set; } = "1IzIM5X_ph99Q_fw9T290IMFOpkZj5Qkc";
    public string SheetName { get; set; } = "Equipos Alquilados";
    public int SyncIntervalMinutes { get; set; } = 15;
    public int HeaderRowIndex { get; set; } = 6;
    public string ClientId { get; set; } = "104519674085578864792";
    public bool Enabled { get; set; } = true;
}
