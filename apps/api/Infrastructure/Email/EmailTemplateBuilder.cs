namespace PlataformaActas.Api.Infrastructure.Email;

/// <summary>
/// Generador centralizado de plantillas HTML institucionales para notificaciones de actas.
/// </summary>
public static class EmailTemplateBuilder
{
    /// <summary>
    /// Construye el contenido HTML responsivo del correo de emisión de acta oficial firmada.
    /// </summary>
    public static string BuildActaNotificationHtml(ActaEmailNotification notification)
    {
        string fechaFirmaStr;
        try
        {
            var tzId = OperatingSystem.IsWindows() ? "SA Pacific Standard Time" : "America/Bogota";
            var tzColombia = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var utcTime = DateTime.SpecifyKind(notification.FechaFirmaUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
            var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzColombia);
            fechaFirmaStr = fechaLocal.ToString("dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            var utcTime = DateTime.SpecifyKind(notification.FechaFirmaUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
            fechaFirmaStr = utcTime.AddHours(-5).ToString("dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        }

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <style>
            body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f6f8; color: #1e293b; }
            .container { max-width: 600px; margin: 24px auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); border: 1px solid #e2e8f0; }
            .header { background-color: #1e5ba8; padding: 24px 32px; text-align: left; color: #ffffff; }
            .header h1 { margin: 0; font-size: 20px; font-weight: 700; letter-spacing: -0.025em; }
            .header p { margin: 4px 0 0 0; font-size: 13px; opacity: 0.9; }
            .content { padding: 32px; }
            .greeting { font-size: 16px; font-weight: 600; margin-bottom: 12px; }
            .intro { font-size: 14px; line-height: 1.6; color: #475569; margin-bottom: 24px; }
            .details-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; font-size: 13px; background-color: #f8fafc; border-radius: 6px; overflow: hidden; border: 1px solid #e2e8f0; }
            .details-table td { padding: 10px 14px; border-bottom: 1px solid #e2e8f0; }
            .details-table tr:last-child td { border-bottom: none; }
            .details-label { font-weight: 600; color: #64748b; width: 38%; }
            .details-value { color: #0f172a; font-weight: 500; }
            .footer { background-color: #f8fafc; padding: 20px 32px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
            .footer p { margin: 4px 0; }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>Assist Consultores de Sistemas S.A.</h1>
              <p>Plataforma de Gestión de Actas y Activos TI</p>
            </div>
            <div class="content">
              <div class="greeting">Estimado(a) {{System.Net.WebUtility.HtmlEncode(notification.DestinatarioNombre)}},</div>
              <p class="intro">
                Le informamos que se ha completado y firmado formalmente el <strong>Acta de {{System.Net.WebUtility.HtmlEncode(notification.TipoActa)}}</strong> con número oficial <strong>{{System.Net.WebUtility.HtmlEncode(notification.NumeroActa)}}</strong>.
              </p>
              
              <table class="details-table">
                <tr>
                  <td class="details-label">Número de Acta:</td>
                  <td class="details-value"><strong>{{System.Net.WebUtility.HtmlEncode(notification.NumeroActa)}}</strong></td>
                </tr>
                <tr>
                  <td class="details-label">Tipo de Movimiento:</td>
                  <td class="details-value">{{System.Net.WebUtility.HtmlEncode(notification.TipoActa)}}</td>
                </tr>
                <tr>
                  <td class="details-label">Colaborador Receptor:</td>
                  <td class="details-value">{{System.Net.WebUtility.HtmlEncode(notification.DestinatarioNombre)}}</td>
                </tr>
                <tr>
                  <td class="details-label">Activo Tecnológico:</td>
                  <td class="details-value">{{System.Net.WebUtility.HtmlEncode(notification.EquipoMarca)}} (Serial / Placa: {{System.Net.WebUtility.HtmlEncode(notification.EquipoSerial)}})</td>
                </tr>
                <tr>
                  <td class="details-label">Fecha y Hora de Firma:</td>
                  <td class="details-value">{{fechaFirmaStr}}</td>
                </tr>
              </table>
            </div>
            <div class="footer">
              <p><strong>Assist Consultores de Sistemas S.A.</strong> &bull; Soporte de Tecnología e Infraestructura</p>
              <p>Este es un mensaje generado automáticamente por la Plataforma de Actas. Por favor no responda a este correo.</p>
            </div>
          </div>
        </body>
        </html>
        """;
    }
}
