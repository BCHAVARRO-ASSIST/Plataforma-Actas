namespace PlataformaActas.Api.Features.Actas;

public sealed record ActaResponse(
    Guid Id,
    string NumeroActa,
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

public sealed record ActaDetailResponse(
    Guid Id,
    string NumeroActa,
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

// =====================================================
// SOLICITUD PARA FIRMAR ACTA
// =====================================================

public sealed record FirmarActaRequest(
    string SignatureDataJson
);

// =====================================================
// SOLICITUD PARA ANULAR ACTA
// =====================================================

public sealed record AnularActaRequest(
    string Motivo
);