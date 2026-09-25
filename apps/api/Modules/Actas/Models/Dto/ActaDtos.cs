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
    string? MotivoAnulacion,
    string? ObservacionesEntrega = null,
    string? ObservacionesDevolucion = null
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
    EquipmentActaResponse Equipo,
    string? ObservacionesEntrega = null,
    string? ObservacionesDevolucion = null,
    string? DestinatarioDocumento = null
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
    Guid? ActaOrigenId,
    string? ObservacionesEntrega = null,
    string? ObservacionesDevolucion = null
);

public sealed record FirmarActaRequest(
    string SignatureDataJson,
    string? Observacion = null,
    List<string>? CorreosNotificacionAdicionales = null,
    string? ObservacionesDevolucion = null,
    string? ObservacionesEntrega = null
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

public sealed record AccesorioEstadoDto(
    bool Entregado = false,
    bool Devuelto = false
);

public static class AccesoriosConstantes
{
    public const string Mouse = "Mouse";
    public const string Teclado = "Teclado";
    public const string Base = "Base";
    public const string Monitor = "Monitor";
    public const string Guaya = "Guaya";
    public const string Maletin = "Maletín";
    public const string DiscoDuroExterno = "Disco duro externo";

    public static readonly string[] Todos =
    [
        Mouse,
        Teclado,
        Base,
        Monitor,
        Guaya,
        Maletin,
        DiscoDuroExterno
    ];

    public static string NormalizarClave(string? clave) => (clave?.Trim().ToLowerInvariant()) switch
    {
        "mouse" => Mouse,
        "teclado" => Teclado,
        "base" => Base,
        "monitor" => Monitor,
        "guaya" => Guaya,
        "maletin" or "maletín" => Maletin,
        "discoduroexterno" or "disco duro externo" or "discoduro" or "disco_duro_externo" => DiscoDuroExterno,
        _ => clave ?? string.Empty
    };
}
