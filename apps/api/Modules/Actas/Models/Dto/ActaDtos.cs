using System;
using System.Text.Json;
using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Models.Dto;

public sealed record ActaResponse(
    Guid Id,
    string? NumeroActa,
    Guid EquipmentId,
    string? InventoryKey,
    string? NombreEquipo,
    int UsuarioId,
    int? DestinatarioUsuarioId,
    int? MensajeroUsuarioId,
    bool ActaEntregada,
    string Estado,
    string TipoActa,
    Guid? ActaOrigenId,
    string? RutaDocumento,
    DateTimeOffset? FechaFirma,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaAnulacion,
    DateTimeOffset? FechaFinalizacion,
    string? DeviceId,
    string FormDataJson,
    string? HashSha256,
    string? MotivoAnulacion
);

public sealed record EquipmentActaResponse(
    Guid Id,
    string InventoryKey,
    string NombreEquipo,
    string? Empresa,
    string? AsignadoA,
    string? Proveedor,
    string? Marca,
    string? Procesador,
    string? DiscoDuro,
    string? MemoriaRam,
    string? Departamento,
    string? EstatusEquipo
);

public sealed record ActaDetailResponse(
    Guid Id,
    string? NumeroActa,
    string Estado,
    string TipoActa,
    Guid? ActaOrigenId,
    string? ActaOrigenNumero,
    bool ActaEntregada,
    DateTimeOffset FechaCreacion,
    DateTimeOffset? FechaFirma,
    DateTimeOffset? FechaFinalizacion,
    DateTimeOffset? FechaAnulacion,
    int UsuarioId,
    int? DestinatarioUsuarioId,
    string? DestinatarioNombre,
    string? DestinatarioCorreo,
    string? DestinatarioCargo,
    int? MensajeroUsuarioId,
    string? MensajeroNombre,
    string? MensajeroCorreo,
    string? DeviceId,
    string FormDataJson,
    string? SignatureDataJson,
    string? HashSha256,
    string? MotivoAnulacion,
    EquipmentActaResponse Equipo
);

public sealed record CreateActaRequest(
    Guid EquipmentId,
    int UsuarioId,
    int? DestinatarioUsuarioId,
    int? MensajeroUsuarioId,
    bool ActaEntregada,
    string? DeviceId,
    string FormDataJson,
    string TipoActa,
    Guid? ActaOrigenId
);

public sealed record FirmarActaRequest(
    string SignatureDataJson,
    string? Observacion = null,
    List<string>? CorreosNotificacionAdicionales = null
);

public sealed record AnularActaRequest(
    string Motivo
);

public sealed record DocumentoActaResult(
    byte[] PdfBytes,
    string FileName,
    string ContentType,
    string? RutaDocumento,
    string? HashSha256,
    DateTimeOffset FechaGeneracion
);

public sealed record GenerarDocumentoResponse(
    Guid ActaId,
    string? NumeroActa,
    string RutaDocumento,
    string? HashSha256,
    DateTimeOffset FechaGeneracion,
    long TamanoBytes
);
