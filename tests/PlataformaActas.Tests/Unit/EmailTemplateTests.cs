using System;
using System.Collections.Generic;
using PlataformaActas.Api.Infrastructure.Email;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public class EmailTemplateTests
{
    private static ActaEmailNotification CreateSampleNotification(DateTime? fechaFirmaUtc = null)
    {
        return new ActaEmailNotification(
            ToEmails: new List<string> { "usuario@empresa.com" },
            CcEmails: new List<string>(),
            BccEmails: new List<string>(),
            DestinatarioNombre: "Juan Perez",
            NumeroActa: "P8-FR-003-ACTA-2026-0005",
            TipoActa: "Entrega",
            EquipoSerial: "ABC-12345",
            EquipoMarca: "Lenovo ThinkPad",
            PdfBytes: new byte[] { 1, 2, 3 },
            FechaFirmaUtc: fechaFirmaUtc ?? new DateTime(2026, 9, 18, 18, 40, 52, DateTimeKind.Utc)
        );
    }

    [Fact]
    public void BuildHtmlTemplate_DoesNotContainTechnicalOrLegalBoxes()
    {
        var notification = CreateSampleNotification();
        var html = SmtpEmailSenderService.BuildHtmlTemplate(notification);

        Assert.DoesNotContain("notice-box", html);
        Assert.DoesNotContain("SHA-256", html);
        Assert.DoesNotContain("ISO/IEC 27001", html);
        Assert.DoesNotContain("inmutable", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ley 603", html);
    }

    [Fact]
    public void BuildHtmlTemplate_UsesCorporateBlueHeaderAndCleanContainer()
    {
        var notification = CreateSampleNotification();
        var html = SmtpEmailSenderService.BuildHtmlTemplate(notification);

        Assert.Contains("#1e5ba8", html);
        Assert.Contains("max-width: 600px", html);
    }

    [Fact]
    public void BuildHtmlTemplate_ContainsCorrectTableLabelsWithoutTypos()
    {
        var notification = CreateSampleNotification();
        var html = SmtpEmailSenderService.BuildHtmlTemplate(notification);

        Assert.Contains("Activo Tecnológico:", html);
        Assert.DoesNotContain("Aotivo Teonológico", html);
    }

    [Fact]
    public void BuildHtmlTemplate_FormatsColombiaTimeZoneCorrectly()
    {
        // 18:40:52 UTC = 13:40:52 (01:40:52 PM) en Colombia (UTC-5)
        var utcDate = new DateTime(2026, 9, 18, 18, 40, 52, DateTimeKind.Utc);
        var notification = CreateSampleNotification(utcDate);
        var html = SmtpEmailSenderService.BuildHtmlTemplate(notification);

        Assert.Contains("18/09/2026 01:40:52 PM", html);
        Assert.DoesNotContain("18:40:52 UTC", html);
    }
}
