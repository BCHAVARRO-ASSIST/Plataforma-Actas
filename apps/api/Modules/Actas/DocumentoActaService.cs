using System.Globalization;
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

/// <summary>
/// Contrato del servicio de generación, custodia e inmutabilidad de documentos PDF de actas.
/// </summary>
public interface IDocumentoActaService
{
    /// <summary>
    /// Renderiza en memoria el documento PDF oficial según la especificación formal del formato P8-FR-003.
    /// </summary>
    /// <param name="acta">Datos estructurados y desnormalizados del acta, hardware y actores.</param>
    /// <returns>Arreglo de bytes binarios del archivo PDF generado.</returns>
    byte[] GenerarPdfBytes(ActaDetailResponse acta);

    /// <summary>
    /// Genera el documento PDF formal, computa su hash SHA-256 de inmutabilidad y lo persiste físicamente en almacenamiento.
    /// </summary>
    /// <param name="actaId">Identificador global del acta a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Metadatos del documento persistido incluyendo ruta relativa y firma criptográfica SHA-256.</returns>
    Task<GenerarDocumentoResponse> GenerarYGuardarPdfAsync(Guid actaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera el documento PDF de un acta aplicando una estrategia de lectura directa desde disco o renderizado por demanda.
    /// </summary>
    /// <param name="actaId">Identificador global del acta.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Resultado con el flujo binario, nombre de archivo y metadatos de integridad.</returns>
    Task<DocumentoActaResult> ObtenerOVisualizarPdfAsync(Guid actaId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Motor de renderizado y certificación documental de actas institucionales de Assist Consultores.
/// </summary>
/// <remarks>
/// <para>
/// <b>Normativa y Formato P8-FR-003:</b>
/// Este servicio materializa el formato corporativo oficial <i>P8-FR-003</i> ("Entrega y Devolución de Equipos"),
/// dando estricto cumplimiento a la <b>Ley 603 de 2000</b> de la República de Colombia (control de bienes corporativos y
/// licencias de software) y a los controles de gestión de activos de la norma <b>ISO/IEC 27001</b>.
/// </para>
/// <para>
/// <b>Estrategia de Optimización de I/O y CPU:</b>
/// <list type="bullet">
///   <item><description><b>Caché en Memoria del Logotipo:</b> La imagen corporativa se carga una única vez mediante <see cref="Lazy{T}"/>, eliminando lecturas redundantes a disco en cada compilación de acta.</description></item>
///   <item><description><b>Resolución Agnóstica de Zona Horaria:</b> Estandariza la visualización cronológica en hora oficial colombiana (COT / UTC-5) garantizando consistencia idéntica entre entornos de despliegue Windows y Linux (Docker Alpine).</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class DocumentoActaService : IDocumentoActaService
{
    private readonly PlataformaActasDbContext _context;
    private readonly IFileStorageProvider _fileStorageProvider;

    // 1. CACHÉ EN MEMORIA DEL LOGOTIPO CORPORATIVO (0 I/O recurrente)
    private static readonly Lazy<byte[]?> CachedLogo = new(() =>
    {
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "assist-logo.png");
        if (File.Exists(logoPath))
        {
            try
            {
                return File.ReadAllBytes(logoPath);
            }
            catch { /* Fallback a texto vectorial si el archivo no está accesible */ }
        }
        return null;
    });

    // 2. ZONA HORARIA OFICIAL COLOMBIA (UTC-5)
    private static readonly TimeZoneInfo ColombiaTimeZone = ResolveColombiaTimeZone();
    private static readonly CultureInfo ColombiaCulture = new("es-CO");

    private static TimeZoneInfo ResolveColombiaTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Bogota"); }
        catch { }

        try { return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); }
        catch { }

        return TimeZoneInfo.CreateCustomTimeZone("COT", TimeSpan.FromHours(-5), "Colombia Time", "Colombia Standard Time");
    }

    public DocumentoActaService(
        PlataformaActasDbContext context,
        IFileStorageProvider fileStorageProvider)
    {
        _context = context;
        _fileStorageProvider = fileStorageProvider;
    }

    private sealed record AssistActaData(
        string ObservacionEntrega,
        string ObservacionDevolucion,
        string DocumentoIdentidad,
        string Ciudad,
        string ProyectoProceso,
        string SerialMonitor,
        string SerialCargador,
        string MarcaMonitor,
        string DireccionMac,
        string SistemaOperativo,
        string ValorElementos,
        HashSet<string> AccesoriosEntregados,
        HashSet<string> AccesoriosDevueltos
    );

    /// <summary>
    /// Compila el modelo declarativo QuestPDF en bytes binarios con base en la estructura tabular oficial P8-FR-003.
    /// </summary>
    /// <param name="acta">Datos consolidados del acta, hardware y firmas.</param>
    /// <returns>Bytes binarios correspondientes al PDF maquetado.</returns>
    /// <remarks>
    /// Ajusta dinámicamente las secciones según el tipo de trámite (<c>Entrega</c> o <c>Devolución</c>), estampando
    /// las cláusulas contractuales de responsabilidad, la matriz de accesorios periféricos y la rúbrica biométrica si existe.
    /// </remarks>
    public byte[] GenerarPdfBytes(ActaDetailResponse acta)
    {
        var data = ParseAssistData(acta.FormDataJson, acta);
        var signatureImageBytes = ParseSignature(acta.SignatureDataJson);

        var isDevolucion = string.Equals(acta.TipoActa, "Devolución", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(acta.TipoActa, "Devolucion", StringComparison.OrdinalIgnoreCase);

        // Fechas normalizadas en hora colombiana (UTC-5)
        var fechaEmisionCo = TimeZoneInfo.ConvertTime(acta.FechaCreacion, ColombiaTimeZone);
        var fechaEmisionStr = fechaEmisionCo.ToString("dd/MM/yyyy", ColombiaCulture);

        string fechaDevolucionStr = "—";
        if (isDevolucion)
        {
            var fechaDev = acta.FechaFinalizacion.HasValue
                ? TimeZoneInfo.ConvertTime(acta.FechaFinalizacion.Value, ColombiaTimeZone)
                : TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ColombiaTimeZone);
            fechaDevolucionStr = fechaDev.ToString("dd/MM/yyyy", ColombiaCulture);
        }

        var nombreDestinatario = acta.DestinatarioNombre ?? acta.Equipo.AsignadoA ?? "Colaborador Asignado";
        var documentoCc = !string.IsNullOrWhiteSpace(data.DocumentoIdentidad) ? data.DocumentoIdentidad : "—";
        var ciudad = !string.IsNullOrWhiteSpace(data.Ciudad) ? data.Ciudad : (acta.Equipo.Empresa ?? "Bogotá D.C.");
        var proyectoProceso = !string.IsNullOrWhiteSpace(data.ProyectoProceso)
            ? data.ProyectoProceso
            : (acta.DestinatarioCargo ?? acta.Equipo.Departamento ?? "Tecnología");

        var marcaEquipo = !string.IsNullOrWhiteSpace(acta.Equipo.Marca) ? acta.Equipo.Marca : acta.Equipo.NombreEquipo;
        var cpuSerial = !string.IsNullOrWhiteSpace(acta.Equipo.InventoryKey) ? acta.Equipo.InventoryKey : "—";

        var assistBlue = Color.FromHex("#4B88BE");
        var assistBlueLight = Color.FromHex("#EDF5FB");
        var assistGreyBorder = Color.FromHex("#CBD5E1");

        var assistLogoBytes = CachedLogo.Value;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginHorizontal(32);
                page.MarginVertical(22);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("DejaVu Sans").FontColor(Colors.Grey.Darken4));

                page.Header().Element(header =>
                {
                    header.PaddingBottom(8).Border(1).BorderColor(Colors.Black).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(170).BorderRight(1).BorderColor(Colors.Black).Padding(4).AlignCenter().AlignMiddle().Element(el =>
                            {
                                if (assistLogoBytes is { Length: > 0 })
                                {
                                    el.MaxHeight(32).Image(assistLogoBytes).FitArea();
                                }
                                else
                                {
                                    el.Row(logoRow =>
                                    {
                                        logoRow.AutoItem().Width(24).Height(24).Background(Colors.Red.Medium)
                                            .AlignCenter().AlignMiddle().Text("A").Bold().FontColor(Colors.White).FontSize(13);

                                        logoRow.AutoItem().PaddingLeft(6).AlignMiddle().Text("ASSIST")
                                            .FontSize(15).Bold().FontColor(Colors.BlueGrey.Darken4);
                                    });
                                }
                            });

                            r.RelativeItem().Padding(6).AlignCenter().AlignMiddle().Text("ACTA DE ENTREGA Y DEVOLUCIÓN")
                                .FontSize(11).Bold().FontColor(Colors.Black);
                        });

                        col.Item().BorderTop(1).BorderColor(Colors.Black).Row(subRow =>
                        {
                            subRow.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Padding(3).AlignCenter()
                                .Text("Código: P8-FR-003").FontSize(7.5f).Bold();

                            subRow.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Padding(3).AlignCenter()
                                .Text("Versión: 2").FontSize(7.5f);

                            subRow.RelativeItem(1.4f).BorderRight(1).BorderColor(Colors.Black).Padding(3).AlignCenter()
                                .Text("Fecha de actualización: 04/09/2024").FontSize(7.5f);

                            subRow.RelativeItem().Padding(3).AlignCenter().Text(t =>
                            {
                                t.Span("Página ").FontSize(7.5f);
                                t.CurrentPageNumber().FontSize(7.5f);
                                t.Span(" de ").FontSize(7.5f);
                                t.TotalPages().FontSize(7.5f);
                            });
                        });
                    });
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(6).Text(txt =>
                    {
                        txt.Span("El área de soporte de Assist Consultores de Sistemas S.A.: ");
                        txt.Span("Entrega ").Bold();
                        txt.Span(isDevolucion ? "[   ]  " : "[ X ]  ").Bold().FontColor(isDevolucion ? Colors.Grey.Darken1 : Colors.Blue.Darken3);
                        txt.Span("Recibe ").Bold();
                        txt.Span(isDevolucion ? "[ X ]  " : "[   ]  ").Bold().FontColor(isDevolucion ? Colors.Blue.Darken3 : Colors.Grey.Darken1);
                        txt.Span("el equipo de cómputo y/o accesorios con los siguientes datos anexos:");
                    });

                    col.Item().Row(mainGrid =>
                    {
                        mainGrid.RelativeItem(1.05f).PaddingRight(5).Table(hwTable =>
                        {
                            hwTable.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(95);
                                cd.RelativeColumn();
                            });

                            hwTable.Cell().ColumnSpan(2).Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3.5f)
                                .AlignCenter().Text("ESPECIFICACIONES DEL HARDWARE").Bold().FontColor(Colors.White).FontSize(7.5f);

                            void RenderHwRow(string label, string? value, bool isEven)
                            {
                                var bg = isEven ? assistBlueLight : Colors.White;
                                hwTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .Text(label).Bold().FontSize(7.2f);
                                hwTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .Text(value ?? "—").FontSize(7.2f);
                            }

                            RenderHwRow("CPU Serial:", cpuSerial, false);
                            RenderHwRow("Monitor Serial:", data.SerialMonitor, true);
                            RenderHwRow("Cargador Serial:", data.SerialCargador, false);
                            RenderHwRow("Marca equipo:", marcaEquipo, true);
                            RenderHwRow("Marca monitor:", data.MarcaMonitor, false);
                            RenderHwRow("MAC:", data.DireccionMac, true);
                            RenderHwRow("Sistema Operativo:", data.SistemaOperativo, false);
                            RenderHwRow("Procesador:", acta.Equipo.Procesador, true);
                            RenderHwRow("Memoria Ram:", acta.Equipo.MemoriaRam, false);
                            RenderHwRow("Disco Duro:", acta.Equipo.DiscoDuro, true);
                            RenderHwRow("Proveedor:", acta.Equipo.Proveedor ?? "Assist Consultores", false);
                        });

                        mainGrid.RelativeItem(1.1f).PaddingLeft(5).Column(rightCol =>
                        {
                            rightCol.Item().Table(userTable =>
                            {
                                userTable.ColumnsDefinition(cd =>
                                {
                                    cd.ConstantColumn(110);
                                    cd.RelativeColumn();
                                });

                                userTable.Cell().ColumnSpan(2).Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3.5f)
                                    .AlignCenter().Text("INFORMACIÓN USUARIO (Quien recibe / devuelve el equipo)").Bold().FontColor(Colors.White).FontSize(7.5f);

                                void RenderUserRow(string label, string value, bool isEven)
                                {
                                    var bg = isEven ? assistBlueLight : Colors.White;
                                    userTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .Text(label).Bold().FontSize(7.2f);
                                    userTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .Text(value).FontSize(7.2f);
                                }

                                RenderUserRow("Nombre del Usuario:", nombreDestinatario, false);
                                RenderUserRow("Identificación (C.C):", documentoCc, true);
                                RenderUserRow("Fecha de entrega:", fechaEmisionStr, false);
                                RenderUserRow("Fecha devolución:", fechaDevolucionStr, true);
                                RenderUserRow("Proyecto y/o Proceso:", proyectoProceso, false);
                                RenderUserRow("Ciudad:", ciudad, true);
                            });

                            rightCol.Item().PaddingTop(5).Table(accTable =>
                            {
                                accTable.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1.8f);
                                    cd.RelativeColumn(1f);
                                    cd.RelativeColumn(1f);
                                });

                                accTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .Text("ACCESORIOS").Bold().FontColor(Colors.White).FontSize(7.2f);

                                accTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .AlignCenter().Text("Entregado").Bold().FontColor(Colors.White).FontSize(7.2f);

                                accTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .AlignCenter().Text("Devuelto").Bold().FontColor(Colors.White).FontSize(7.2f);

                                var items = new[] { "Mouse", "Teclado", "Base", "Monitor", "Guaya", "Maletín", "Disco duro externo" };
                                for (int i = 0; i < items.Length; i++)
                                {
                                    var item = items[i];
                                    var bg = (i % 2 == 1) ? assistBlueLight : Colors.White;
                                    var isEntregado = data.AccesoriosEntregados.Contains(item) || (!isDevolucion && data.AccesoriosEntregados.Count == 0 && (item == "Cargador" || item == "Maletín"));
                                    var isDevuelto = data.AccesoriosDevueltos.Contains(item) || (isDevolucion && isEntregado);

                                    accTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2f)
                                        .Text(item).FontSize(7.2f);

                                    accTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2f)
                                        .AlignCenter().Text(isEntregado ? "[ X ]" : "[   ]").FontSize(7.2f).Bold();

                                    accTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2f)
                                        .AlignCenter().Text(isDevuelto ? "[ X ]" : "[   ]").FontSize(7.2f).Bold();
                                }
                            });
                        });
                    });

                    col.Item().PaddingTop(6);

                    col.Item().Row(secRow =>
                    {
                        secRow.RelativeItem(1.05f).PaddingRight(5).Column(swCol =>
                        {
                            swCol.Item().Text("Listado de programas instalados, si necesita agregar un programa favor comunicar al área de soporte para su proceso, si el usuario instala programa(s) sin la debida autorización del área encargada, es responsabilidad del usuario del equipo ante una eventual auditoria. Ley 603 del 2000, Derecho de Propiedad Intelectual.")
                                .FontSize(6.2f).Italic().FontColor(Colors.Grey.Darken3);

                            swCol.Item().PaddingTop(3).Table(swTable =>
                            {
                                swTable.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1.8f);
                                    cd.RelativeColumn(1f);
                                    cd.RelativeColumn(1f);
                                });

                                swTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .Text("LISTADO PROGRAMAS").Bold().FontColor(Colors.White).FontSize(7.2f);
                                swTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .AlignCenter().Text("INSTALADO").Bold().FontColor(Colors.White).FontSize(7.2f);
                                swTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                    .AlignCenter().Text("VERSION").Bold().FontColor(Colors.White).FontSize(7.2f);

                                var softwares = new (string Name, bool Installed, string Version)[]
                                {
                                    ("IIB 10 / Broker 9/8/7", false, ""),
                                    ("Adobe Reader", true, "DC"),
                                    ("Soap UI", false, ""),
                                    ("Microsoft Office", true, "365"),
                                    ("Tortoise SVN", false, ""),
                                    ("Microsoft Project", false, ""),
                                    ("Microsoft Visio", false, ""),
                                    ("VmWare", false, ""),
                                    ("7zip", true, "24.0"),
                                    ("SQL Server", false, ""),
                                    ("Avast Antivirus", true, "Corporate"),
                                    ("Otro Software:", false, "")
                                };

                                for (int i = 0; i < softwares.Length; i++)
                                {
                                    var s = softwares[i];
                                    var bg = (i % 2 == 1) ? assistBlueLight : Colors.White;
                                    swTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2f)
                                        .Text(s.Name).FontSize(7.2f);
                                    swTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2f)
                                        .AlignCenter().Text(s.Installed ? "[ X ]" : "[   ]").FontSize(7.2f).Bold();
                                    swTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2f)
                                        .AlignCenter().Text(s.Version).FontSize(7.2f);
                                }
                            });
                        });

                        secRow.RelativeItem(1.1f).PaddingLeft(5).Column(obsCol =>
                        {
                            obsCol.Item().Text("Observaciones del estado de entrega / devolución del equipo:")
                                .FontSize(7.2f).Bold();

                            obsCol.Item().PaddingTop(2).Table(obsTable =>
                            {
                                obsTable.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn();
                                    cd.RelativeColumn();
                                });

                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Background(assistBlueLight).Padding(2f)
                                    .Text("Entrega:").Bold().FontSize(7.2f);
                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Background(assistBlueLight).Padding(2f)
                                    .Text("Devolución:").Bold().FontSize(7.2f);

                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Padding(3).MinHeight(26)
                                    .Text(data.ObservacionEntrega).FontSize(6.8f);
                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Padding(3).MinHeight(26)
                                    .Text(data.ObservacionDevolucion).FontSize(6.8f);
                            });

                            obsCol.Item().PaddingTop(4).Text("AUTORIZACIÓN DE DESCUENTO POR NÓMINA O LIQUIDACIÓN EN CASO DE NO DEVOLUCIÓN, PÉRDIDA O DAÑO DE ELEMENTOS ENTREGADOS.")
                                .FontSize(7f).Bold().FontColor(Colors.Black);

                            obsCol.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span($"Yo, {nombreDestinatario} identificado(a) con C.C. No. {documentoCc}, por medio de la presente declaro que he recibido en condiciones óptimas y funcionales los elementos relacionados con anterioridad.\n").FontSize(6.4f);
                                t.Span("Reconozco que estos elementos son propiedad de ").FontSize(6.4f);
                                t.Span("ASSIST CONSULTORES DE SISTEMAS S.A ").Bold().FontSize(6.4f);
                                t.Span("y que han sido entregados para el desarrollo de mis funciones laborales. Me comprometo a mantener en buen estado y hacer uso adecuado de dichos elementos.\n").FontSize(6.4f);
                                t.Span("En caso de no devolución, pérdida, hurto, daño o deterioro que no sea atribuible al desgaste normal por el uso, autorizo expresamente a ").FontSize(6.4f);
                                t.Span("ASSIST CONSULTORES DE SISTEMAS S.A ").Bold().FontSize(6.4f);
                                t.Span("para que, previa investigación interna, se descuente el valor de los elementos de mi salario, prestaciones o liquidación final.").FontSize(6.4f);
                            });

                            obsCol.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Black).Padding(2.5f).Row(vRow =>
                            {
                                vRow.AutoItem().Text("Valor de los Elementos: ").Bold().FontSize(7.2f);
                                vRow.RelativeItem().Text(data.ValorElementos).FontSize(7.2f);
                            });
                        });
                    });

                    col.Item().PaddingTop(6);

                    // CONTENCIÓN ESTRICTA DE SALTOS: Asegura que el bloque de firmas no se fracture
                    col.Item().ShowEntire().Row(signGrid =>
                    {
                        signGrid.RelativeItem().PaddingRight(6).Column(entCol =>
                        {
                            entCol.Item().Text("Firmas de Entrega:").Bold().FontSize(7.2f);

                            entCol.Item().PaddingTop(2).Row(er =>
                            {
                                er.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(3).Column(sc =>
                                {
                                    sc.Item().Text("Entrega:").Bold().FontSize(6.8f);
                                    sc.Item().Height(34).AlignMiddle().AlignCenter()
                                        .Text("Andrés Trujillo").FontFamily("Courier").FontSize(9.5f).Bold().FontColor(Colors.Blue.Darken3);
                                    sc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    sc.Item().PaddingTop(2).Text("Nombre: Andrés Trujillo").FontSize(6.5f);
                                    sc.Item().Text("Cargo: Mesa de ayuda").FontSize(6.5f);
                                });

                                er.ConstantItem(5);

                                er.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(3).Column(rc =>
                                {
                                    rc.Item().Text("Recibe:").Bold().FontSize(6.8f);
                                    rc.Item().Height(34).AlignMiddle().AlignCenter().Element(el =>
                                    {
                                        if (signatureImageBytes is { Length: > 0 })
                                        {
                                            el.Image(signatureImageBytes).FitArea();
                                        }
                                        else
                                        {
                                            el.Text("Pendiente de firma").FontSize(6.8f).Italic().FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                    rc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    rc.Item().PaddingTop(2).Text($"Nombre: {nombreDestinatario}").Bold().FontSize(6.5f);
                                    rc.Item().Text($"Cargo: {proyectoProceso}").FontSize(6.5f);
                                });
                            });
                        });

                        signGrid.RelativeItem().PaddingLeft(6).Column(devCol =>
                        {
                            devCol.Item().Text("Firmas de Devolución:").Bold().FontSize(7.2f);
                            devCol.Item().Text("Soporte recibe a conformidad:  Si [   ]   No [   ]").FontSize(6.5f);

                            devCol.Item().PaddingTop(2).Row(dr =>
                            {
                                dr.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(3).Column(sc =>
                                {
                                    sc.Item().Text("Entrega:").Bold().FontSize(6.8f);
                                    sc.Item().Height(34);
                                    sc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    sc.Item().PaddingTop(2).Text("Nombre: _________________").FontSize(6.5f);
                                    sc.Item().Text("Cargo: _________________").FontSize(6.5f);
                                });

                                dr.ConstantItem(5);

                                dr.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(3).Column(rc =>
                                {
                                    rc.Item().Text("Recibe:").Bold().FontSize(6.8f);
                                    rc.Item().Height(34);
                                    rc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    rc.Item().PaddingTop(2).Text("Nombre: _________________").FontSize(6.5f);
                                    rc.Item().Text("Cargo: _________________").FontSize(6.5f);
                                });
                            });
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static AssistActaData ParseAssistData(string? formDataJson, ActaDetailResponse acta)
    {
        var obsEntrega = "Pendiente inspección física en laboratorio.";
        var obsDevolucion = "—";
        var docCc = "—";
        var ciudad = "Bogotá D.C.";
        var proyecto = acta.DestinatarioCargo ?? acta.Equipo.Departamento ?? "Tecnología";
        var serialMonitor = "—";
        var serialCargador = "—";
        var marcaMonitor = "—";
        var mac = "—";
        var so = "Windows 11 Pro";
        var valor = "$ 3.500.000 COP (Valor estimado en inventario)";
        var accEntregados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accDevueltos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(formDataJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(formDataJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("observacion", out var o1) && o1.ValueKind == JsonValueKind.String)
                {
                    obsEntrega = o1.GetString() ?? obsEntrega;
                }
                else if (root.TryGetProperty("observaciones", out var o2) && o2.ValueKind == JsonValueKind.String)
                {
                    obsEntrega = o2.GetString() ?? obsEntrega;
                }

                if (root.TryGetProperty("observacionDevolucion", out var od) && od.ValueKind == JsonValueKind.String)
                {
                    obsDevolucion = od.GetString() ?? obsDevolucion;
                }

                if (root.TryGetProperty("cedula", out var c1) && c1.ValueKind == JsonValueKind.String)
                {
                    docCc = c1.GetString() ?? docCc;
                }
                else if (root.TryGetProperty("documento", out var c2) && c2.ValueKind == JsonValueKind.String)
                {
                    docCc = c2.GetString() ?? docCc;
                }

                if (root.TryGetProperty("ciudad", out var ci) && ci.ValueKind == JsonValueKind.String)
                {
                    ciudad = ci.GetString() ?? ciudad;
                }

                if (root.TryGetProperty("proyecto", out var pr) && pr.ValueKind == JsonValueKind.String)
                {
                    proyecto = pr.GetString() ?? proyecto;
                }

                if (root.TryGetProperty("serialMonitor", out var sm) && sm.ValueKind == JsonValueKind.String)
                {
                    serialMonitor = sm.GetString() ?? serialMonitor;
                }

                if (root.TryGetProperty("serialCargador", out var sc) && sc.ValueKind == JsonValueKind.String)
                {
                    serialCargador = sc.GetString() ?? serialCargador;
                }

                if (root.TryGetProperty("marcaMonitor", out var mm) && mm.ValueKind == JsonValueKind.String)
                {
                    marcaMonitor = mm.GetString() ?? marcaMonitor;
                }

                if (root.TryGetProperty("mac", out var mc) && mc.ValueKind == JsonValueKind.String)
                {
                    mac = mc.GetString() ?? mac;
                }

                if (root.TryGetProperty("sistemaOperativo", out var sop) && sop.ValueKind == JsonValueKind.String)
                {
                    so = sop.GetString() ?? so;
                }

                if (root.TryGetProperty("valorElementos", out var val) && val.ValueKind == JsonValueKind.String)
                {
                    valor = val.GetString() ?? valor;
                }

                if (root.TryGetProperty("accesorios", out var acc) && acc.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in acc.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                        {
                            accEntregados.Add(item.GetString()!);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        return new AssistActaData(
            obsEntrega,
            obsDevolucion,
            docCc,
            ciudad,
            proyecto,
            serialMonitor,
            serialCargador,
            marcaMonitor,
            mac,
            so,
            valor,
            accEntregados,
            accDevueltos
        );
    }

    /// <summary>
    /// Orquesta la generación del PDF oficial, el cálculo de su hash inmutable SHA-256 y la persistencia en disco.
    /// </summary>
    /// <param name="actaId">Identificador único global del acta.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Metadatos del documento generado y guardado.</returns>
    /// <exception cref="KeyNotFoundException">Se arroja si el identificador del acta no existe en el sistema.</exception>
    /// <remarks>
    /// Este método se invoca durante el proceso de firma. El cálculo del hash criptográfico SHA-256 garantiza
    /// el principio de inalterabilidad y no repudio: cualquier alteración posterior del archivo binario invalidaría
    /// la correspondencia con el hash registrado en la base de datos relacional.
    /// </remarks>
    public async Task<GenerarDocumentoResponse> GenerarYGuardarPdfAsync(
        Guid actaId,
        CancellationToken cancellationToken = default)
    {
        var acta = await _context.Actas
            .Include(a => a.Equipo)
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

    /// <summary>
    /// Proporciona acceso al documento PDF del acta, priorizando la lectura directa del binario sellado en almacenamiento.
    /// </summary>
    /// <param name="actaId">Identificador único del acta consultada.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Resultado con el binario del PDF y metadatos de cabecera HTTP para descarga o previsualización.</returns>
    /// <exception cref="KeyNotFoundException">Se arroja si el acta no existe.</exception>
    /// <remarks>
    /// <para>
    /// <b>Estrategia de Almacenamiento Directo:</b>
    /// Si el acta ya posee una <c>RutaDocumento</c> persistida (estado <c>Firmada</c> o posterior), el método recupera
    /// directamente los bytes del proveedor de almacenamiento (<see cref="IFileStorageProvider"/>).
    /// Esto previene el costo computacional de recompilar el árbol visual de QuestPDF en cada visualización concurrente
    /// y asegura que el usuario siempre descargue exactamente el mismo archivo binario sellado con el hash SHA-256 original.
    /// Si aún es un borrador no firmado, se sintetiza una vista preliminar al vuelo.
    /// </para>
    /// </remarks>
    public async Task<DocumentoActaResult> ObtenerOVisualizarPdfAsync(
        Guid actaId,
        CancellationToken cancellationToken = default)
    {
        var acta = await _context.Actas
            .Include(a => a.Equipo)
            .Include(a => a.Usuario)
            .Include(a => a.DestinatarioUsuario)
            .Include(a => a.MensajeroUsuario)
            .Include(a => a.ActaOrigen)
            .FirstOrDefaultAsync(a => a.Id == actaId, cancellationToken);

        if (acta is null)
        {
            throw new KeyNotFoundException($"El acta con ID '{actaId}' no fue encontrada.");
        }

        var fileName = $"Acta_ASSIST_{SanitizeFileName(acta.NumeroActa ?? acta.Id.ToString())}.pdf";

        // Si el archivo ya fue generado y persistido en almacenamiento, servirlo directamente
        if (!string.IsNullOrWhiteSpace(acta.RutaDocumento))
        {
            var existingBytes = await _fileStorageProvider.GetActaDocumentAsync(acta.RutaDocumento, cancellationToken);
            if (existingBytes is { Length: > 0 })
            {
                return new DocumentoActaResult(
                    PdfBytes: existingBytes,
                    FileName: fileName,
                    ContentType: "application/pdf",
                    RutaDocumento: acta.RutaDocumento,
                    HashSha256: acta.HashSha256,
                    FechaGeneracion: acta.FechaFirma ?? acta.FechaCreacion
                );
            }
        }

        // Si no existe aún en disco, generar por demanda
        var detail = acta.ToDetailResponse();
        var pdfBytes = GenerarPdfBytes(detail);

        return new DocumentoActaResult(
            PdfBytes: pdfBytes,
            FileName: fileName,
            ContentType: "application/pdf",
            RutaDocumento: acta.RutaDocumento,
            HashSha256: acta.HashSha256,
            FechaGeneracion: DateTimeOffset.UtcNow
        );
    }

    /// <summary>
    /// Extrae y valida rigurosamente la rúbrica manuscrita en Base64 verificando firmas mágicas binarias.
    /// </summary>
    /// <param name="signatureDataJson">Payload JSON o cadena Data-URI con la imagen capturada en el canvas.</param>
    /// <returns>Arreglo de bytes del gráfico validado o <c>null</c> si el contenido no es una imagen válida.</returns>
    /// <remarks>
    /// <para>
    /// <b>Defensa en Profundidad y Verificación de Cabeceras Mágicas:</b>
    /// Por seguridad y estabilidad del renderizador de documentos, no basta con decodificar Base64.
    /// Se valida que la carga útil posea al menos 64 bytes (descartando trazos vacíos) y que sus primeros bytes
    /// correspondan inequívocamente a los números mágicos de formato PNG (<c>0x89 50 4E 47</c>) o JPEG (<c>0xFF D8</c>).
    /// Esto previene excepciones de renderizado en QuestPDF o inyecciones de payloads arbitrarios.
    /// </para>
    /// </remarks>
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

            var bytes = Convert.FromBase64String(base64);

            // Mínimo de bytes para evitar artefactos vacíos o rotos
            if (bytes.Length < 64)
            {
                return null;
            }

            // Validar cabecera mágica PNG (0x89 0x50 0x4E 0x47) o JPEG (0xFF 0xD8)
            var isPng = bytes.Length > 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
            var isJpeg = bytes.Length > 2 && bytes[0] == 0xFF && bytes[1] == 0xD8;

            return (isPng || isJpeg) ? bytes : null;
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
