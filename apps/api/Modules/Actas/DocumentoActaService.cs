using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Infrastructure.Security;
using PlataformaActas.Api.Infrastructure.Storage;
using PlataformaActas.Api.Modules.Actas.Models.Dto;
using PlataformaActas.Api.Modules.Actas.Models.Mappers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlataformaActas.Api.Modules.Actas.Services;

public sealed class DocumentoActaService : IDocumentoActaService
{
    private readonly PlataformaActasDbContext _context;
    private readonly IFileStorageProvider _fileStorageProvider;

    public DocumentoActaService(
        PlataformaActasDbContext context,
        IFileStorageProvider fileStorageProvider)
    {
        _context = context;
        _fileStorageProvider = fileStorageProvider;
    }

    public byte[] GenerarPdfBytes(ActaDetailResponse acta)
    {
        var (observaciones, accesorios) = ParseFormData(acta.FormDataJson);
        var signatureImageBytes = ParseSignature(acta.SignatureDataJson);

        var isDevolucion = string.Equals(acta.TipoActa, "Devolución", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(acta.TipoActa, "Devolucion", StringComparison.OrdinalIgnoreCase);

        var tituloActa = isDevolucion
            ? "ACTA OFICIAL DE DEVOLUCIÓN Y RECEPCIÓN DE EQUIPO TECNOLÓGICO"
            : "ACTA OFICIAL DE ENTREGA Y ASIGNACIÓN DE EQUIPO TECNOLÓGICO";

        var fechaEmision = acta.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt");
        var fechaFirmaStr = acta.FechaFirma.HasValue
            ? acta.FechaFirma.Value.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt")
            : "Pendiente de firma";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                // ENCABEZADO INSTITUCIONAL
                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(c =>
                            {
                                c.Item().Text("PLATAFORMA DE GESTIÓN Y CONTROL DE ACTIVOS TECNOLÓGICOS")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.BlueGrey.Darken4);

                                c.Item().Text("DEPARTAMENTO DE TECNOLOGÍA E INFRAESTRUCTURA (TI)")
                                    .FontSize(8.5f)
                                    .Bold()
                                    .FontColor(Colors.Grey.Darken2);
                            });

                            row.RelativeItem(1.5f).AlignRight().Column(c =>
                            {
                                c.Item().Text("CÓDIGO: TI-FOR-ACTA-001").FontSize(7.5f).Bold();
                                c.Item().Text("VERSIÓN: 3.2 · ISO 27001").FontSize(7.5f);
                                c.Item().Text(x =>
                                {
                                    x.Span("PÁGINA: ").FontSize(7.5f);
                                    x.CurrentPageNumber().FontSize(7.5f);
                                    x.Span(" de ").FontSize(7.5f);
                                    x.TotalPages().FontSize(7.5f);
                                });
                            });
                        });

                        col.Item().PaddingTop(6).PaddingBottom(8).LineHorizontal(1.5f).LineColor(Colors.BlueGrey.Darken4);
                    });
                });

                // CONTENIDO PRINCIPAL
                page.Content().Column(col =>
                {
                    // BLOQUE TÍTULO
                    col.Item().Background(Colors.Grey.Lighten4)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(8)
                        .Column(c =>
                        {
                            c.Item().AlignCenter().Text(tituloActa)
                                .FontSize(11)
                                .Bold()
                                .FontColor(Colors.BlueGrey.Darken4);

                            c.Item().AlignCenter().Row(r =>
                            {
                                r.AutoItem().Text("N° DE CONTROL: ").FontSize(8.5f).Bold();
                                r.AutoItem().Text(acta.NumeroActa ?? "PENDIENTE").FontSize(8.5f).Bold().FontColor(Colors.Blue.Darken3);

                                if (!string.IsNullOrWhiteSpace(acta.ActaOrigenNumero))
                                {
                                    r.AutoItem().PaddingLeft(12).Text($"(Acta de Entrega de Origen: {acta.ActaOrigenNumero})")
                                        .FontSize(8.5f).Bold().FontColor(Colors.Grey.Darken2);
                                }
                            });
                        });

                    col.Item().PaddingTop(10);

                    // SECCIÓN 1: INFORMACIÓN DE LA OPERACIÓN
                    col.Item().Text("1. INFORMACIÓN DE LA OPERACIÓN Y PARTICIPANTES")
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.BlueGrey.Darken4);

                    col.Item().PaddingTop(3).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(130);
                            columns.RelativeColumn();
                            columns.ConstantColumn(130);
                            columns.RelativeColumn();
                        });

                        // Fila 1
                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Fecha de Emisión:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(fechaEmision).FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Estado del Acta:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Estado.ToUpperInvariant()).Bold().FontSize(8).FontColor(Colors.Blue.Darken2);

                        // Fila 2
                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Receptor / Consultor:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.DestinatarioNombre ?? acta.Equipo.AsignadoA ?? "Consultor Asignado").Bold().FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Correo / Cargo:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{acta.DestinatarioCorreo ?? "—"} ({acta.DestinatarioCargo ?? acta.Equipo.Departamento ?? "Tecnología"})").FontSize(8);

                        // Fila 3
                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Responsable Entrega / Mensajero:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.MensajeroNombre ?? "Personal de Logística TI").FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Empresa / Sede:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.Empresa ?? "Sede Principal").FontSize(8);
                    });

                    col.Item().PaddingTop(10);

                    // SECCIÓN 2: ESPECIFICACIONES TÉCNICAS DEL HARDWARE
                    col.Item().Text("2. ESPECIFICACIONES TÉCNICAS DEL HARDWARE Y ACCESORIOS")
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.BlueGrey.Darken4);

                    col.Item().PaddingTop(3).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(130);
                            columns.RelativeColumn();
                            columns.ConstantColumn(130);
                            columns.RelativeColumn();
                        });

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Código Inventario / Serie:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.InventoryKey).Bold().FontSize(8).FontColor(Colors.Blue.Darken3);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Nombre / Modelo:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.NombreEquipo).Bold().FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Marca:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.Marca ?? "—").FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Procesador:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.Procesador ?? "—").FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Memoria RAM:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.MemoriaRam ?? "—").FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Almacenamiento:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.DiscoDuro ?? "—").FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Departamento:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.Departamento ?? "Tecnología").FontSize(8);

                        table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text("Estado Operativo:").Bold().FontSize(8);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(acta.Equipo.EstatusEquipo ?? "Activo").Bold().FontSize(8).FontColor(Colors.Green.Darken3);

                        if (accesorios.Count > 0)
                        {
                            table.Cell().Background(Colors.Grey.Lighten4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text("Accesorios Entregados:").Bold().FontSize(8);
                            table.Cell().ColumnSpan(3).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(string.Join(" · ", accesorios)).FontSize(8);
                        }
                    });

                    col.Item().PaddingTop(10);

                    // SECCIÓN 3: OBSERVACIONES
                    col.Item().Text("3. OBSERVACIONES DE ENTREGA / CONDICIÓN FÍSICA")
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.BlueGrey.Darken4);

                    col.Item().PaddingTop(3)
                        .Background(Colors.Grey.Lighten4)
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(6)
                        .Text(observaciones)
                        .FontSize(8);

                    col.Item().PaddingTop(10);

                    // SECCIÓN 4: TÉRMINOS Y COMPROMISOS
                    col.Item().Text("4. TÉRMINOS, CONDICIONES Y COMPROMISO DE DEVOLUCIÓN")
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.BlueGrey.Darken4);

                    col.Item().PaddingTop(3)
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(6)
                        .Column(c =>
                        {
                            c.Item().Text("A. CUSTODIA Y BUEN USO: El colaborador / consultor declara haber recibido el equipo anteriormente descrito en perfecto estado técnico y funcional, asumiendo su custodia, cuidado y utilización estricta conforme a las políticas de seguridad de la información corporativas.")
                                .FontSize(7.5f);

                            c.Item().PaddingTop(3).Text("B. COMPROMISO FORMAL DE DEVOLUCIÓN: De esta misma acta se derivará el proceso de devolución. El colaborador se compromete expresamente a restituir el mismo activo tecnológico y sus accesorios a la organización cuando finalice su relación contractual, cuando se realice un cambio o cuando el área de TI lo requiera.")
                                .FontSize(7.5f);

                            c.Item().PaddingTop(3).Text("C. VALIDEZ JURÍDICA Y DIGITAL: La firma manuscrita digitalizada estampada en este documento y su hash SHA-256 constituyen plena prueba de recepción y aceptación de los términos establecidos.")
                                .FontSize(7.5f);
                        });

                    col.Item().PaddingTop(10);

                    // SECCIÓN 5: FIRMAS Y CERTIFICACIÓN
                    col.Item().Text("5. FIRMA DE CONFORMIDAD Y REGISTRO DIGITAL")
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.BlueGrey.Darken4);

                    col.Item().PaddingTop(4).Row(row =>
                    {
                        // FIRMA CONSULTOR / RECEPTOR
                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("FIRMA DEL RECEPTOR / CONSULTOR")
                                .FontSize(8).Bold().FontColor(Colors.Grey.Darken2);

                            c.Item().Height(55).AlignCenter().AlignMiddle().Element(el =>
                            {
                                if (signatureImageBytes is { Length: > 0 })
                                {
                                    el.Image(signatureImageBytes).FitArea();
                                }
                                else
                                {
                                    el.Text("Firma pendiente por recolectar")
                                        .FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                                }
                            });

                            c.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                            c.Item().PaddingTop(3).AlignCenter().Text(acta.DestinatarioNombre ?? acta.Equipo.AsignadoA ?? "Consultor / Usuario")
                                .FontSize(8).Bold();
                            c.Item().AlignCenter().Text($"Fecha y Hora de Firma: {fechaFirmaStr}")
                                .FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(12);

                        // FIRMA / SELLO RESPONSABLE TI
                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("RESPONSABLE DE ENTREGA / LOGÍSTICA TI")
                                .FontSize(8).Bold().FontColor(Colors.Grey.Darken2);

                            c.Item().Height(55).AlignCenter().AlignMiddle().Column(sc =>
                            {
                                sc.Item().AlignCenter().Text("🛡️").FontSize(14);
                                sc.Item().AlignCenter().Text("DOCUMENTO AUTORIZADO Y VALIDADO")
                                    .FontSize(7.5f).Bold().FontColor(Colors.Green.Darken3);
                            });

                            c.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                            c.Item().PaddingTop(3).AlignCenter().Text(acta.MensajeroNombre ?? "Operador de Logística TI")
                                .FontSize(8).Bold();
                            c.Item().AlignCenter().Text($"Emisión: {fechaEmision}")
                                .FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // SECCIÓN 6: DILIGENCIAMIENTO AL MOMENTO DE DEVOLUCIÓN (SOLO PARA ENTREGA)
                    if (!isDevolucion)
                    {
                        col.Item().PaddingTop(8)
                            .Border(0.5f)
                            .BorderColor(Colors.Grey.Medium)
                            .Padding(6)
                            .Column(c =>
                            {
                                c.Item().Text("6. DILIGENCIAMIENTO AL MOMENTO DE LA DEVOLUCIÓN FÍSICA FUTURA")
                                    .FontSize(7.5f).Bold().FontColor(Colors.Grey.Darken3);

                                c.Item().PaddingTop(3).Row(r =>
                                {
                                    r.RelativeItem().Text("Fecha Devolución: ____________________").FontSize(7.5f);
                                    r.RelativeItem().Text("Estado: [ ] Óptimo  [ ] Avería").FontSize(7.5f);
                                    r.RelativeItem().Text("Recibido por (Firma TI): ____________________").FontSize(7.5f);
                                });
                            });
                    }
                });

                // PIE DE PÁGINA CON HASH SHA-256
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem(3).Column(c =>
                        {
                            c.Item().Text("INTEGRIDAD CRIPTOGRÁFICA (SHA-256):").FontSize(7).Bold();
                            c.Item().Text(acta.HashSha256 ?? "Documento en borrador previo a firma digital").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem(1.5f).AlignRight().Column(c =>
                        {
                            c.Item().Text("Certificado Digitalmente").FontSize(7).Bold().FontColor(Colors.Green.Darken3);
                            c.Item().Text(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")).FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<GenerarDocumentoResponse> GenerarYGuardarPdfAsync(
        Guid actaId,
        CancellationToken cancellationToken = default)
    {
        var acta = await _context.Actas
            .Include(a => a.Equipment)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.MensajeroUsuario)
            .Include(a => a.ActaOrigen)
            .FirstOrDefaultAsync(a => a.Id == actaId, cancellationToken);

        if (acta is null)
        {
            throw new KeyNotFoundException($"El acta con ID '{actaId}' no fue encontrada.");
        }

        var detail = acta.ToDetailResponse();
        var pdfBytes = GenerarPdfBytes(detail);

        var hash = Sha256IntegrityService.ComputeHash(pdfBytes);
        var numeroActa = acta.NumeroActa ?? "PENDIENTE";

        var relativePath = await _fileStorageProvider.SaveActaDocumentAsync(actaId, numeroActa, pdfBytes, cancellationToken);

        acta.RutaDocumento = relativePath;
        acta.HashSha256 = hash;
        await _context.SaveChangesAsync(cancellationToken);

        return new GenerarDocumentoResponse(
            ActaId: acta.Id,
            NumeroActa: acta.NumeroActa,
            RutaDocumento: relativePath,
            HashSha256: hash,
            FechaGeneracion: DateTimeOffset.UtcNow,
            TamanoBytes: pdfBytes.Length
        );
    }

    public async Task<DocumentoActaResult> ObtenerOVisualizarPdfAsync(
        Guid actaId,
        CancellationToken cancellationToken = default)
    {
        var acta = await _context.Actas
            .Include(a => a.Equipment)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.MensajeroUsuario)
            .Include(a => a.ActaOrigen)
            .FirstOrDefaultAsync(a => a.Id == actaId, cancellationToken);

        if (acta is null)
        {
            throw new KeyNotFoundException($"El acta con ID '{actaId}' no fue encontrada.");
        }

        var fileName = $"Acta_{SanitizeFileName(acta.NumeroActa ?? acta.Id.ToString())}.pdf";

        if (!string.IsNullOrWhiteSpace(acta.RutaDocumento))
        {
            var bytes = await _fileStorageProvider.GetActaDocumentAsync(acta.RutaDocumento, cancellationToken);
            if (bytes != null)
            {
                return new DocumentoActaResult(
                    PdfBytes: bytes,
                    FileName: fileName,
                    ContentType: "application/pdf",
                    RutaDocumento: acta.RutaDocumento,
                    HashSha256: acta.HashSha256,
                    FechaGeneracion: acta.FechaFirma ?? acta.FechaCreacion
                );
            }
        }

        var generated = await GenerarYGuardarPdfAsync(actaId, cancellationToken);
        var genBytes = await _fileStorageProvider.GetActaDocumentAsync(generated.RutaDocumento, cancellationToken);

        return new DocumentoActaResult(
            PdfBytes: genBytes ?? GenerarPdfBytes(acta.ToDetailResponse()),
            FileName: fileName,
            ContentType: "application/pdf",
            RutaDocumento: generated.RutaDocumento,
            HashSha256: generated.HashSha256,
            FechaGeneracion: generated.FechaGeneracion
        );
    }

    private static (string Observaciones, List<string> Accesorios) ParseFormData(string? formDataJson)
    {
        if (string.IsNullOrWhiteSpace(formDataJson))
        {
            return ("Sin observaciones registradas.", []);
        }

        try
        {
            using var doc = JsonDocument.Parse(formDataJson);
            var root = doc.RootElement;

            var observacion = "Sin observaciones registradas.";
            if (root.TryGetProperty("observacion", out var obsProp) && obsProp.ValueKind == JsonValueKind.String)
            {
                observacion = obsProp.GetString() ?? observacion;
            }
            else if (root.TryGetProperty("observaciones", out var obsPluralProp) && obsPluralProp.ValueKind == JsonValueKind.String)
            {
                observacion = obsPluralProp.GetString() ?? observacion;
            }

            var accesorios = new List<string>();
            if (root.TryGetProperty("accesorios", out var accProp) && accProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in accProp.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        accesorios.Add(item.GetString()!);
                    }
                }
            }

            return (observacion, accesorios);
        }
        catch
        {
            return ("Sin observaciones registradas.", []);
        }
    }

    private static byte[]? ParseSignature(string? signatureDataJson)
    {
        if (string.IsNullOrWhiteSpace(signatureDataJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(signatureDataJson);
            var root = doc.RootElement;

            string? base64 = null;
            if (root.TryGetProperty("firma", out var firmaProp) && firmaProp.ValueKind == JsonValueKind.String)
            {
                base64 = firmaProp.GetString();
            }
            else if (root.TryGetProperty("signature", out var sigProp) && sigProp.ValueKind == JsonValueKind.String)
            {
                base64 = sigProp.GetString();
            }

            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            var commaIndex = base64.IndexOf(',');
            if (commaIndex >= 0)
            {
                base64 = base64[(commaIndex + 1)..];
            }

            return Convert.FromBase64String(base64);
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
}
