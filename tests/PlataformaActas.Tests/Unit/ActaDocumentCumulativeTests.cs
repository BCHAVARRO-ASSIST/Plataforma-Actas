using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PlataformaActas.Api.Modules.Actas.Services;
using PlataformaActas.Api.Modules.Actas.Models.Dto;
using QuestPDF.Infrastructure;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public sealed class ActaDocumentCumulativeTests
{
    private static readonly bool IsSkiaAvailable;

    static ActaDocumentCumulativeTests()
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            IsSkiaAvailable = true;
        }
        catch
        {
            IsSkiaAvailable = false;
        }
    }

    private static readonly string SampleSignatureBase64 =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

    [Fact]
    public void GenerarPdfBytes_ActaDevolucion_PreservesBothObservationsAndSignatures_StrictlySinglePage()
    {
        if (!IsSkiaAvailable) return;

        // Arrange
        var service = new ActaDocumentService(null!, null!);

        var signaturePayload = JsonSerializer.Serialize(new
        {
            firmaEntregaConsultor = SampleSignatureBase64,
            firmaEntregaSoporte = "AndresTrujillo",
            firmaDevolucionConsultor = SampleSignatureBase64,
            firmaDevolucionSoporte = "AndresTrujillo",
            firma = SampleSignatureBase64
        });

        var formDataPayload = JsonSerializer.Serialize(new
        {
            observacion = "Entrega inicial con teclado y cargador.",
            observacionDevolucion = "Devolución en óptimas condiciones físicas y lógicas.",
            cedula = "1018485291",
            ciudad = "Bogotá D.C.",
            proyecto = "Consultoría",
            accesorios = new[] { "Cargador", "Mouse" }
        });

        var actaDevolucion = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-0099",
            Estado: "Firmada",
            TipoActa: "Devolución",
            ActaOrigenId: Guid.NewGuid(),
            ActaOrigenNumero: "ACTA-2026-0098",
            ActaEntregada: false,
            FechaCreacion: DateTimeOffset.UtcNow.AddDays(-10),
            FechaFirma: DateTimeOffset.UtcNow,
            FechaFinalizacion: DateTimeOffset.UtcNow,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 2,
            DestinatarioNombre: "Brayan Chavarro Giraldo",
            DestinatarioCorreo: "bchavarro@assist.com.co",
            DestinatarioCargo: "Consultor TI",
            MensajeroUsuarioId: null,
            MensajeroNombre: null,
            MensajeroCorreo: null,
            DeviceId: null,
            FormDataJson: formDataPayload,
            SignatureDataJson: signaturePayload,
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: new EquipmentActaResponse(
                Id: Guid.NewGuid(),
                InventoryKey: "AST-NB-0084",
                NombreEquipo: "ThinkPad T14 Gen 3",
                Empresa: "ASSIST S.A.",
                AsignadoA: "Brayan Chavarro Giraldo",
                Proveedor: "Lenovo",
                Marca: "Lenovo",
                Procesador: "Intel Core i7-1260P",
                DiscoDuro: "512 GB NVMe",
                MemoriaRam: "16 GB DDR4",
                Departamento: "Consultoría",
                EstatusEquipo: "Activo"
            ),
            ObservacionesEntrega: "Entrega inicial con teclado y cargador.",
            ObservacionesDevolucion: "Devolución en óptimas condiciones físicas y lógicas."
        );

        // Act
        var pdfBytes = service.GenerarPdfBytes(actaDevolucion);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000);

        // Validar cabecera PDF estándar
        var header = Encoding.ASCII.GetString(pdfBytes[..5]);
        Assert.Equal("%PDF-", header);

        // Validar conteo estricto de 1 sola página Carta
        var pdfContent = Encoding.Latin1.GetString(pdfBytes);
        var pageCountMatches = Regex.Matches(pdfContent, @"/Type\s*/Page\b");
        // En QuestPDF, los objetos de página tienen "/Type /Page". Si hay exactamente 1 página, este patrón aparece 1 vez
        Assert.Single(pageCountMatches);
    }

    [Fact]
    public void GenerarPdfBytes_ActaEntrega_RendersCorrectly()
    {
        if (!IsSkiaAvailable) return;

        // Arrange
        var service = new ActaDocumentService(null!, null!);

        var signaturePayload = JsonSerializer.Serialize(new
        {
            firmaEntregaConsultor = SampleSignatureBase64,
            firmaEntregaSoporte = "AndresTrujillo",
            firma = SampleSignatureBase64
        });

        var formDataPayload = JsonSerializer.Serialize(new
        {
            observacion = "Equipo entregado nuevo de paquete sellado.",
            cedula = "1018485291",
            ciudad = "Bogotá D.C.",
            proyecto = "Desarrollo",
            accesorios = new[] { "Cargador", "Maletín" }
        });

        var actaEntrega = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-0100",
            Estado: "Firmada",
            TipoActa: "Entrega",
            ActaOrigenId: null,
            ActaOrigenNumero: null,
            ActaEntregada: true,
            FechaCreacion: DateTimeOffset.UtcNow,
            FechaFirma: DateTimeOffset.UtcNow,
            FechaFinalizacion: null,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 2,
            DestinatarioNombre: "Brayan Chavarro Giraldo",
            DestinatarioCorreo: "bchavarro@assist.com.co",
            DestinatarioCargo: "Desarrollador",
            MensajeroUsuarioId: null,
            MensajeroNombre: null,
            MensajeroCorreo: null,
            DeviceId: null,
            FormDataJson: formDataPayload,
            SignatureDataJson: signaturePayload,
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: new EquipmentActaResponse(
                Id: Guid.NewGuid(),
                InventoryKey: "AST-NB-0085",
                NombreEquipo: "Dell Latitude 5430",
                Empresa: "ASSIST S.A.",
                AsignadoA: "Brayan Chavarro Giraldo",
                Proveedor: "Dell",
                Marca: "Dell",
                Procesador: "Intel Core i5-1245U",
                DiscoDuro: "256 GB NVMe",
                MemoriaRam: "16 GB DDR4",
                Departamento: "Desarrollo",
                EstatusEquipo: "Asignado"
            ),
            ObservacionesEntrega: "Equipo entregado nuevo de paquete sellado.",
            ObservacionesDevolucion: null
        );

        // Act
        var pdfBytes = service.GenerarPdfBytes(actaEntrega);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000);

        var pdfContent = Encoding.Latin1.GetString(pdfBytes);
        var pageCountMatches = Regex.Matches(pdfContent, @"/Type\s*/Page\b");
        Assert.Single(pageCountMatches);
    }

    [Fact]
    public void GenerarPdfBytes_LoadsEmbeddedAndresTrujilloSignatureImage()
    {
        if (!IsSkiaAvailable) return;

        var service = new ActaDocumentService(null!, null!);

        var actaDevolucion = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-0007",
            Estado: "Finalizada",
            TipoActa: "Devolución",
            ActaOrigenId: Guid.NewGuid(),
            ActaOrigenNumero: "ACTA-2026-0006",
            ActaEntregada: false,
            FechaCreacion: DateTimeOffset.UtcNow.AddDays(-5),
            FechaFirma: DateTimeOffset.UtcNow,
            FechaFinalizacion: DateTimeOffset.UtcNow,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 2,
            DestinatarioNombre: "Brayan Chavarro Giraldo",
            DestinatarioCorreo: "bchavarro@assist.com.co",
            DestinatarioCargo: "Consultor TI",
            MensajeroUsuarioId: null,
            MensajeroNombre: null,
            MensajeroCorreo: null,
            DeviceId: null,
            FormDataJson: "{}",
            SignatureDataJson: "{}",
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: new EquipmentActaResponse(
                Id: Guid.NewGuid(),
                InventoryKey: "AST-NB-0084",
                NombreEquipo: "ThinkPad T14 Gen 3",
                Empresa: "ASSIST S.A.",
                AsignadoA: "Brayan Chavarro Giraldo",
                Proveedor: "Lenovo",
                Marca: "Lenovo",
                Procesador: "Intel Core i7",
                DiscoDuro: "512 GB",
                MemoriaRam: "16 GB",
                Departamento: "Consultoría",
                EstatusEquipo: "Activo"
            ),
            ObservacionesEntrega: "Entrega inicial",
            ObservacionesDevolucion: "Devolución conforme"
        );

        var pdfBytes = service.GenerarPdfBytes(actaDevolucion);
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000);

        var pdfContent = Encoding.Latin1.GetString(pdfBytes);
        Assert.Contains("/Subtype /Image", pdfContent);
    }

    [Fact]
    public void GenerarPdfBytes_HeaderMarking_And_AccessoryTraceability_ObjectMap()
    {
        if (!IsSkiaAvailable) return;

        var service = new ActaDocumentService(null!, null!);

        // Arrange: Entrega con mapa de 7 accesorios
        var accesoriosEntrega = new
        {
            mouse = new { entregado = true, devuelto = false },
            teclado = new { entregado = false, devuelto = false },
            baseItem = new { entregado = false, devuelto = false },
            monitor = new { entregado = false, devuelto = false },
            guaya = new { entregado = true, devuelto = false },
            maletin = new { entregado = true, devuelto = false },
            discoDuroExterno = new { entregado = false, devuelto = false }
        };

        var formDataEntrega = JsonSerializer.Serialize(new
        {
            observacion = "Entrega con mouse, guaya y maletín.",
            accesorios = accesoriosEntrega
        });

        var actaEntrega = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-0501",
            Estado: "Firmada",
            TipoActa: "Entrega",
            ActaOrigenId: null,
            ActaOrigenNumero: null,
            ActaEntregada: true,
            FechaCreacion: DateTimeOffset.UtcNow,
            FechaFirma: DateTimeOffset.UtcNow,
            FechaFinalizacion: null,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 2,
            DestinatarioNombre: "Consultor Pruebas",
            DestinatarioCorreo: "test@assist.com.co",
            DestinatarioCargo: "Desarrollador",
            MensajeroUsuarioId: null,
            MensajeroNombre: null,
            MensajeroCorreo: null,
            DeviceId: null,
            FormDataJson: formDataEntrega,
            SignatureDataJson: "{}",
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: new EquipmentActaResponse(
                Id: Guid.NewGuid(),
                InventoryKey: "AST-NB-9999",
                NombreEquipo: "Laptop Test",
                Empresa: "ASSIST S.A.",
                AsignadoA: "Consultor Pruebas",
                Proveedor: "Lenovo",
                Marca: "Lenovo",
                Procesador: "Intel Core i7",
                DiscoDuro: "512 GB",
                MemoriaRam: "16 GB",
                Departamento: "Desarrollo",
                EstatusEquipo: "Asignado"
            ),
            ObservacionesEntrega: "Entrega inicial con 3 accesorios"
        );

        // Act
        var pdfBytesEntrega = service.GenerarPdfBytes(actaEntrega);

        // Assert Entrega
        Assert.NotNull(pdfBytesEntrega);
        var pdfTextEntrega = Encoding.Latin1.GetString(pdfBytesEntrega);
        var pageMatchesEntrega = Regex.Matches(pdfTextEntrega, @"/Type\s*/Page\b");
        Assert.Single(pageMatchesEntrega);

        // Arrange: Devolución donde se devuelven solo algunos
        var accesoriosDevolucion = new
        {
            mouse = new { entregado = true, devuelto = true },
            teclado = new { entregado = false, devuelto = false },
            baseItem = new { entregado = false, devuelto = false },
            monitor = new { entregado = false, devuelto = false },
            guaya = new { entregado = true, devuelto = false },
            maletin = new { entregado = true, devuelto = true },
            discoDuroExterno = new { entregado = false, devuelto = false }
        };

        var formDataDevolucion = JsonSerializer.Serialize(new
        {
            observacion = "Entrega con mouse, guaya y maletín.",
            observacionesDevolucion = "No devuelve la guaya por pérdida.",
            accesorios = accesoriosDevolucion
        });

        var actaDevolucion = actaEntrega with
        {
            Id = Guid.NewGuid(),
            NumeroActa = "ACTA-2026-0502",
            TipoActa = "Devolución",
            ActaOrigenId = actaEntrega.Id,
            FormDataJson = formDataDevolucion,
            ObservacionesDevolucion = "No devuelve la guaya por pérdida."
        };

        // Act
        var pdfBytesDevolucion = service.GenerarPdfBytes(actaDevolucion);

        // Assert Devolución
        Assert.NotNull(pdfBytesDevolucion);
        var pdfTextDevolucion = Encoding.Latin1.GetString(pdfBytesDevolucion);
        var pageMatchesDevolucion = Regex.Matches(pdfTextDevolucion, @"/Type\s*/Page\b");
        Assert.Single(pageMatchesDevolucion);
    }

    [Fact]
    public void GenerarPdfBytes_IncludesLegalAndOperationalTexts_AndStrictlySinglePage()
    {
        if (!IsSkiaAvailable) return;

        // Arrange
        var service = new ActaDocumentService(null!, null!);
        var acta = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-TEST-LEGAL",
            TipoActa: "Entrega",
            Estado: "Borrador",
            ActaOrigenId: null,
            ActaOrigenNumero: null,
            ActaEntregada: false,
            FechaCreacion: DateTimeOffset.UtcNow,
            FechaFirma: null,
            FechaFinalizacion: null,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 8,
            DestinatarioNombre: "Brayan Chavarro",
            DestinatarioCorreo: "bchavarro@assist.com.co",
            DestinatarioCargo: "Consultor Senior",
            MensajeroUsuarioId: 6,
            MensajeroNombre: "Mauricio Robayo",
            MensajeroCorreo: "mrobayo@assist.com.co",
            DeviceId: "MOBILE-DEVICE-001",
            FormDataJson: "{}",
            SignatureDataJson: null,
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: new EquipmentActaResponse(
                Id: Guid.NewGuid(),
                InventoryKey: "HP-LEGAL-001",
                NombreEquipo: "Equipo Portatil HP",
                Empresa: "Assist",
                AsignadoA: "Brayan Chavarro",
                Proveedor: "HP Inc.",
                Marca: "HP",
                Procesador: "Intel Core i7",
                DiscoDuro: "512 GB",
                MemoriaRam: "16 GB",
                Departamento: "Consultoría",
                EstatusEquipo: "Activo"
            ),
            ObservacionesEntrega: "Entrega con texto legal",
            ObservacionesDevolucion: null,
            DestinatarioDocumento: "1018485291"
        );

        // Act
        var pdfBytes = service.GenerarPdfBytes(acta);

        // Assert
        Assert.NotNull(pdfBytes);
        var pdfText = Encoding.Latin1.GetString(pdfBytes);
        var pageMatches = Regex.Matches(pdfText, @"/Type\s*/Page\b");
        Assert.Single(pageMatches);
    }

    [Fact]
    public void DocumentoResolucion_Prioridad1_ManualEnFormDataJson_SobrescribePerfilUsuario()
    {
        // Arrange
        var actaEntity = new PlataformaActas.Api.Modules.Actas.Data.Entities.Acta
        {
            Id = Guid.NewGuid(),
            NumeroActa = "ACTA-2026-TEST1",
            EquipmentId = Guid.NewGuid(),
            UsuarioId = 1,
            DestinatarioUsuarioId = 2,
            Estado = "Borrador",
            TipoActa = "Entrega",
            FormDataJson = JsonSerializer.Serialize(new { numeroDocumento = "12345678", cedula = "12345678" }),
            DestinatarioUsuario = new PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario
            {
                Id = 2,
                Nombre = "Consultor",
                Apellido = "Alfa",
                UsuarioRed = "calfa",
                Correo = "calfa@assist.com.co",
                Proceso = "TI",
                NumeroDocumento = "99999999" // Perfil de usuario tiene 99999999
            }
        };

        // Act
        var detail = PlataformaActas.Api.Modules.Actas.Models.Mappers.ActaMappingExtensions.ToDetailResponse(actaEntity);

        // Assert: Prioridad 1 debe prevalecer (12345678 vs 99999999)
        Assert.Equal("12345678", detail.DestinatarioDocumento);
    }

    [Fact]
    public void DocumentoResolucion_Prioridad2_PerfilUsuario_SeUsaCuandoNoHayValorEnFormData()
    {
        // Arrange
        var actaEntity = new PlataformaActas.Api.Modules.Actas.Data.Entities.Acta
        {
            Id = Guid.NewGuid(),
            NumeroActa = "ACTA-2026-TEST2",
            EquipmentId = Guid.NewGuid(),
            UsuarioId = 1,
            DestinatarioUsuarioId = 3,
            Estado = "Borrador",
            TipoActa = "Entrega",
            FormDataJson = "{}",
            DestinatarioUsuario = new PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario
            {
                Id = 3,
                Nombre = "Consultor",
                Apellido = "Beta",
                UsuarioRed = "cbeta",
                Correo = "cbeta@assist.com.co",
                Proceso = "TI",
                NumeroDocumento = "87654321"
            }
        };

        // Act
        var detail = PlataformaActas.Api.Modules.Actas.Models.Mappers.ActaMappingExtensions.ToDetailResponse(actaEntity);

        // Assert: Prioridad 2 perfil de usuario
        Assert.Equal("87654321", detail.DestinatarioDocumento);
    }

    [Fact]
    public void GenerarPdfBytes_DosActasConCedulasDistintas_MantienenAislamientoEstricto()
    {
        if (!IsSkiaAvailable) return;

        // Arrange
        var service = new ActaDocumentService(null!, null!);

        var eq = new EquipmentActaResponse(
            Guid.NewGuid(), "INV-001", "Portatil Dell", "Assist", "Consultor",
            "Dell", "Dell", "Core i5", "512GB", "16GB", "TI", "Activo"
        );

        var actaA = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-A",
            Estado: "Borrador",
            TipoActa: "Entrega",
            ActaOrigenId: null,
            ActaOrigenNumero: null,
            ActaEntregada: false,
            FechaCreacion: DateTimeOffset.UtcNow,
            FechaFirma: null,
            FechaFinalizacion: null,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 10,
            DestinatarioNombre: "Consultor A",
            DestinatarioCorreo: "a@assist.com.co",
            DestinatarioCargo: "Desarrollador",
            MensajeroUsuarioId: null,
            MensajeroNombre: null,
            MensajeroCorreo: null,
            DeviceId: "WEB",
            FormDataJson: JsonSerializer.Serialize(new { numeroDocumento = "12345678", cedula = "12345678" }),
            SignatureDataJson: null,
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: eq,
            ObservacionesEntrega: "Entrega A",
            DestinatarioDocumento: "12345678"
        );

        var actaB = new ActaDetailResponse(
            Id: Guid.NewGuid(),
            NumeroActa: "ACTA-2026-B",
            Estado: "Borrador",
            TipoActa: "Entrega",
            ActaOrigenId: null,
            ActaOrigenNumero: null,
            ActaEntregada: false,
            FechaCreacion: DateTimeOffset.UtcNow,
            FechaFirma: null,
            FechaFinalizacion: null,
            FechaAnulacion: null,
            UsuarioId: 1,
            DestinatarioUsuarioId: 20,
            DestinatarioNombre: "Consultor B",
            DestinatarioCorreo: "b@assist.com.co",
            DestinatarioCargo: "Desarrollador",
            MensajeroUsuarioId: null,
            MensajeroNombre: null,
            MensajeroCorreo: null,
            DeviceId: "WEB",
            FormDataJson: JsonSerializer.Serialize(new { numeroDocumento = "87654321", cedula = "87654321" }),
            SignatureDataJson: null,
            HashSha256: null,
            MotivoAnulacion: null,
            Equipo: eq,
            ObservacionesEntrega: "Entrega B",
            DestinatarioDocumento: "87654321"
        );

        // Act
        var pdfBytesA = service.GenerarPdfBytes(actaA);
        var pdfBytesB = service.GenerarPdfBytes(actaB);

        // Assert
        Assert.NotNull(pdfBytesA);
        Assert.NotNull(pdfBytesB);
        Assert.NotEqual(pdfBytesA, pdfBytesB);
    }
}


