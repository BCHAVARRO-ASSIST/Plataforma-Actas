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

public interface IDocumentoActaService
{
    byte[] GenerarPdfBytes(ActaDetailResponse acta);
    Task<GenerarDocumentoResponse> GenerarYGuardarPdfAsync(Guid actaId, CancellationToken cancellationToken = default);
    Task<DocumentoActaResult> ObtenerOVisualizarPdfAsync(Guid actaId, CancellationToken cancellationToken = default);
}

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

    public byte[] GenerarPdfBytes(ActaDetailResponse acta)
    {
        var data = ParseAssistData(acta.FormDataJson, acta);
        var signatureImageBytes = ParseSignature(acta.SignatureDataJson);

        var isDevolucion = string.Equals(acta.TipoActa, "Devolución", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(acta.TipoActa, "Devolucion", StringComparison.OrdinalIgnoreCase);

        var fechaEmisionStr = acta.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy");
        var fechaDevolucionStr = isDevolucion
            ? (acta.FechaFinalizacion?.ToLocalTime().ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"))
            : "—";

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

        byte[]? assistLogoBytes = null;
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "assist-logo.png");
        if (File.Exists(logoPath))
        {
            try
            {
                assistLogoBytes = File.ReadAllBytes(logoPath);
            }
            catch { /* fallback */ }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginHorizontal(32);
                page.MarginVertical(24);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial").FontColor(Colors.Grey.Darken4));

                page.Header().Element(header =>
                {
                    header.PaddingBottom(10).Border(1).BorderColor(Colors.Black).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(170).BorderRight(1).BorderColor(Colors.Black).Padding(4).AlignCenter().AlignMiddle().Element(el =>
                            {
                                if (assistLogoBytes is { Length: > 0 })
                                {
                                    el.MaxHeight(34).Image(assistLogoBytes).FitArea();
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
                    col.Item().PaddingBottom(8).Text(txt =>
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
                        mainGrid.RelativeItem(1.05f).PaddingRight(6).Table(hwTable =>
                        {
                            hwTable.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(95);
                                cd.RelativeColumn();
                            });

                            hwTable.Cell().ColumnSpan(2).Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(4)
                                .AlignCenter().Text("ESPECIFICACIONES DEL HARDWARE").Bold().FontColor(Colors.White).FontSize(7.5f);

                            void RenderHwRow(string label, string? value, bool isEven)
                            {
                                var bg = isEven ? assistBlueLight : Colors.White;
                                hwTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .Text(label).Bold().FontSize(7.5f);
                                hwTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .Text(value ?? "—").FontSize(7.5f);
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

                        mainGrid.RelativeItem(1.1f).PaddingLeft(6).Column(rightCol =>
                        {
                            rightCol.Item().Table(userTable =>
                            {
                                userTable.ColumnsDefinition(cd =>
                                {
                                    cd.ConstantColumn(110);
                                    cd.RelativeColumn();
                                });

                                userTable.Cell().ColumnSpan(2).Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(4)
                                    .AlignCenter().Text("INFORMACIÓN USUARIO (Quien recibe / devuelve el equipo)").Bold().FontColor(Colors.White).FontSize(7.5f);

                                void RenderUserRow(string label, string value, bool isEven)
                                {
                                    var bg = isEven ? assistBlueLight : Colors.White;
                                    userTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                        .Text(label).Bold().FontSize(7.5f);
                                    userTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                        .Text(value).FontSize(7.5f);
                                }

                                RenderUserRow("Nombre del Usuario:", nombreDestinatario, false);
                                RenderUserRow("Identificación (C.C):", documentoCc, true);
                                RenderUserRow("Fecha de entrega:", fechaEmisionStr, false);
                                RenderUserRow("Fecha devolución:", fechaDevolucionStr, true);
                                RenderUserRow("Proyecto y/o Proceso:", proyectoProceso, false);
                                RenderUserRow("Ciudad:", ciudad, true);
                            });

                            rightCol.Item().PaddingTop(6).Table(accTable =>
                            {
                                accTable.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1.8f);
                                    cd.RelativeColumn(1f);
                                    cd.RelativeColumn(1f);
                                });

                                accTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .Text("ACCESORIOS").Bold().FontColor(Colors.White).FontSize(7.5f);

                                accTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .AlignCenter().Text("Entregado").Bold().FontColor(Colors.White).FontSize(7.5f);

                                accTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .AlignCenter().Text("Devuelto").Bold().FontColor(Colors.White).FontSize(7.5f);

                                var items = new[] { "Mouse", "Teclado", "Base", "Monitor", "Guaya", "Maletín", "Disco duro externo" };
                                for (int i = 0; i < items.Length; i++)
                                {
                                    var item = items[i];
                                    var bg = (i % 2 == 1) ? assistBlueLight : Colors.White;
                                    var isEntregado = data.AccesoriosEntregados.Contains(item) || (!isDevolucion && data.AccesoriosEntregados.Count == 0 && (item == "Cargador" || item == "Maletín"));
                                    var isDevuelto = data.AccesoriosDevueltos.Contains(item) || (isDevolucion && isEntregado);

                                    accTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .Text(item).FontSize(7.5f);

                                    accTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .AlignCenter().Text(isEntregado ? "[ X ]" : "[   ]").FontSize(7.5f).Bold();

                                    accTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .AlignCenter().Text(isDevuelto ? "[ X ]" : "[   ]").FontSize(7.5f).Bold();
                                }
                            });
                        });
                    });

                    col.Item().PaddingTop(10);

                    col.Item().Row(secRow =>
                    {
                        secRow.RelativeItem(1.05f).PaddingRight(6).Column(swCol =>
                        {
                            swCol.Item().Text("Listado de programas instalados, si necesita agregar un programa favor comunicar al área de soporte para su proceso, si el usuario instala programa(s) sin la debida autorización del área encargada, es responsabilidad del usuario del equipo ante una eventual auditoria. Ley 603 del 2000, Derecho de Propiedad Intelectual.")
                                .FontSize(6.5f).Italic().FontColor(Colors.Grey.Darken3);

                            swCol.Item().PaddingTop(4).Table(swTable =>
                            {
                                swTable.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(1.8f);
                                    cd.RelativeColumn(1f);
                                    cd.RelativeColumn(1f);
                                });

                                swTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .Text("LISTADO PROGRAMAS").Bold().FontColor(Colors.White).FontSize(7.5f);
                                swTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .AlignCenter().Text("INSTALADO").Bold().FontColor(Colors.White).FontSize(7.5f);
                                swTable.Cell().Background(assistBlue).Border(0.5f).BorderColor(assistGreyBorder).Padding(3)
                                    .AlignCenter().Text("VERSION").Bold().FontColor(Colors.White).FontSize(7.5f);

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
                                    swTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .Text(s.Name).FontSize(7.5f);
                                    swTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .AlignCenter().Text(s.Installed ? "[ X ]" : "[   ]").FontSize(7.5f).Bold();
                                    swTable.Cell().Background(bg).Border(0.5f).BorderColor(assistGreyBorder).Padding(2.5f)
                                        .AlignCenter().Text(s.Version).FontSize(7.5f);
                                }
                            });
                        });

                        secRow.RelativeItem(1.1f).PaddingLeft(6).Column(obsCol =>
                        {
                            obsCol.Item().Text("Observaciones del estado de entrega / devolución del equipo:")
                                .FontSize(7.5f).Bold();

                            obsCol.Item().PaddingTop(2).Table(obsTable =>
                            {
                                obsTable.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn();
                                    cd.RelativeColumn();
                                });

                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Background(assistBlueLight).Padding(2.5f)
                                    .Text("Entrega:").Bold().FontSize(7.5f);
                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Background(assistBlueLight).Padding(2.5f)
                                    .Text("Devolución:").Bold().FontSize(7.5f);

                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Padding(4).MinHeight(32)
                                    .Text(data.ObservacionEntrega).FontSize(7);
                                obsTable.Cell().Border(0.5f).BorderColor(assistGreyBorder).Padding(4).MinHeight(32)
                                    .Text(data.ObservacionDevolucion).FontSize(7);
                            });

                            obsCol.Item().PaddingTop(6).Text("AUTORIZACIÓN DE DESCUENTO POR NÓMINA O LIQUIDACIÓN EN CASO DE NO DEVOLUCIÓN, PÉRDIDA O DAÑO DE ELEMENTOS ENTREGADOS.")
                                .FontSize(7.5f).Bold().FontColor(Colors.Black);

                            obsCol.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span($"Yo, {nombreDestinatario} identificado(a) con cédula de ciudadanía No. {documentoCc}, por medio de la presente declaro que he recibido en condiciones óptimas y funcionales los elementos relacionados con anterioridad.\n\n").FontSize(6.8f);
                                t.Span("Reconozco que estos elementos son propiedad de ").FontSize(6.8f);
                                t.Span("ASSIST CONSULTORES DE SISTEMAS S.A ").Bold().FontSize(6.8f);
                                t.Span("y que han sido entregados para el desarrollo de mis funciones laborales. Me comprometo a mantener en buen estado y hacer uso adecuado de dichos elementos.\n\n").FontSize(6.8f);
                                t.Span("En caso de la no devolución, pérdida, hurto fuera de las instalaciones del cliente, oficina u hogar de los elementos entregados, daño o deterioro que no sea atribuible al desgaste normal por el uso, autorizo expresamente a ").FontSize(6.8f);
                                t.Span("ASSIST CONSULTORES DE SISTEMAS S.A ").Bold().FontSize(6.8f);
                                t.Span("para que, después de realizar una investigación interna que determine mi responsabilidad, se proceda a realizar el descuento correspondiente del valor de los elementos de mi salario, prestaciones sociales, liquidación final de prestaciones, o cualquier otro pago que me deba la empresa.").FontSize(6.8f);
                            });

                            obsCol.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Black).Padding(3).Row(vRow =>
                            {
                                vRow.AutoItem().Text("Valor de los Elementos: ").Bold().FontSize(7.5f);
                                vRow.RelativeItem().Text(data.ValorElementos).FontSize(7.5f);
                            });

                            obsCol.Item().PaddingTop(3).Text("Esta autorización se hace en cumplimiento de las normas laborales vigentes y es válida durante el tiempo que dure mi relación laboral con ASSIST CONSULTORES DE SISTEMAS S.A. He sido informado y entiendo completamente las condiciones establecidas en esta cláusula.")
                                .FontSize(6.5f).Italic();
                        });
                    });

                    col.Item().PaddingTop(8);

                    col.Item().Row(signGrid =>
                    {
                        signGrid.RelativeItem().PaddingRight(8).Column(entCol =>
                        {
                            entCol.Item().Text("Firmas de Entrega:").Bold().FontSize(7.5f);

                            entCol.Item().PaddingTop(3).Row(er =>
                            {
                                er.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(4).Column(sc =>
                                {
                                    sc.Item().Text("Entrega:").Bold().FontSize(7);
                                    sc.Item().Height(36).AlignMiddle().AlignCenter()
                                        .Text("Andrés Trujillo").FontFamily("Courier").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                                    sc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    sc.Item().PaddingTop(2).Text("Nombre: Andrés Trujillo").FontSize(6.8f);
                                    sc.Item().Text("Cargo: Mesa de ayuda").FontSize(6.8f);
                                });

                                er.ConstantItem(6);

                                er.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(4).Column(rc =>
                                {
                                    rc.Item().Text("Recibe:").Bold().FontSize(7);
                                    rc.Item().Height(36).AlignMiddle().AlignCenter().Element(el =>
                                    {
                                        if (signatureImageBytes is { Length: > 0 })
                                        {
                                            el.Image(signatureImageBytes).FitArea();
                                        }
                                        else
                                        {
                                            el.Text("Pendiente de firma").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                    rc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    rc.Item().PaddingTop(2).Text($"Nombre: {nombreDestinatario}").Bold().FontSize(6.8f);
                                    rc.Item().Text($"Cargo: {proyectoProceso}").FontSize(6.8f);
                                });
                            });
                        });

                        signGrid.RelativeItem().PaddingLeft(8).Column(devCol =>
                        {
                            devCol.Item().Text("Firmas de Devolución:").Bold().FontSize(7.5f);
                            devCol.Item().Text("El equipo de soporte de la organización recibe a conformidad el equipo devuelto:  Si [   ]   No [   ]")
                                .FontSize(6.8f);

                            devCol.Item().PaddingTop(3).Row(dr =>
                            {
                                dr.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(4).Column(sc =>
                                {
                                    sc.Item().Text("Entrega:").Bold().FontSize(7);
                                    sc.Item().Height(36);
                                    sc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    sc.Item().PaddingTop(2).Text("Nombre: _________________").FontSize(6.8f);
                                    sc.Item().Text("Cargo: _________________").FontSize(6.8f);
                                });

                                dr.ConstantItem(6);

                                dr.RelativeItem().Border(0.5f).BorderColor(assistGreyBorder).Padding(4).Column(rc =>
                                {
                                    rc.Item().Text("Recibe:").Bold().FontSize(7);
                                    rc.Item().Height(36);
                                    rc.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                                    rc.Item().PaddingTop(2).Text("Nombre: _________________").FontSize(6.8f);
                                    rc.Item().Text("Cargo: _________________").FontSize(6.8f);
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

        var fileName = $"Acta_ASSIST_{SanitizeFileName(acta.NumeroActa ?? acta.Id.ToString())}.pdf";

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
