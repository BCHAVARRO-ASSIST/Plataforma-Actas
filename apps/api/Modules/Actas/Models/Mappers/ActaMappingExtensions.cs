using System;
using System.Text.Json;
using PlataformaActas.Api.Modules.Actas.Data.Entities;
using PlataformaActas.Api.Modules.Actas.Models.Dto;

namespace PlataformaActas.Api.Modules.Actas.Models.Mappers;

public static class ActaMappingExtensions
{
    public static ActaResponse ToResponse(this Acta acta)
    {
        return new ActaResponse(
            Id: acta.Id,
            NumeroActa: acta.NumeroActa,
            EquipmentId: acta.EquipmentId,
            InventoryKey: acta.Equipment?.InventoryKey,
            NombreEquipo: acta.Equipment?.NombreEquipo,
            UsuarioId: acta.UsuarioId,
            DestinatarioUsuarioId: acta.DestinatarioUsuarioId,
            MensajeroUsuarioId: acta.MensajeroUsuarioId,
            ActaEntregada: acta.ActaEntregada,
            Estado: acta.Estado,
            TipoActa: acta.TipoActa,
            ActaOrigenId: acta.ActaOrigenId,
            RutaDocumento: acta.RutaDocumento,
            FechaFirma: acta.FechaFirma,
            FechaCreacion: acta.FechaCreacion,
            FechaAnulacion: acta.FechaAnulacion,
            FechaFinalizacion: acta.FechaFinalizacion,
            DeviceId: acta.DeviceId,
            FormDataJson: acta.FormDataJson ?? "{}",
            HashSha256: acta.HashSha256,
            MotivoAnulacion: acta.MotivoAnulacion
        );
    }

    public static ActaDetailResponse ToDetailResponse(this Acta acta)
    {
        var equipoResponse = acta.Equipment != null
            ? new EquipmentActaResponse(
                Id: acta.Equipment.Id,
                InventoryKey: acta.Equipment.InventoryKey,
                NombreEquipo: acta.Equipment.NombreEquipo,
                Empresa: acta.Equipment.Empresa,
                AsignadoA: acta.Equipment.AsignadoA,
                Proveedor: acta.Equipment.Proveedor,
                Marca: acta.Equipment.Marca,
                Procesador: acta.Equipment.Procesador,
                DiscoDuro: acta.Equipment.DiscoDuro,
                MemoriaRam: acta.Equipment.MemoriaRam,
                Departamento: acta.Equipment.Departamento,
                EstatusEquipo: acta.Equipment.EstatusEquipo
            )
            : new EquipmentActaResponse(
                Guid.Empty, string.Empty, string.Empty, null, null, null, null, null, null, null, null, null
            );

        string? destinatarioCargo = null;
        if (!string.IsNullOrWhiteSpace(acta.FormDataJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(acta.FormDataJson);
                if (doc.RootElement.TryGetProperty("destinatarioCargo", out var cargoElem))
                {
                    destinatarioCargo = cargoElem.GetString();
                }
            }
            catch
            {
                // Ignorar error de deserialización de cargo
            }
        }

        return new ActaDetailResponse(
            Id: acta.Id,
            NumeroActa: acta.NumeroActa,
            Estado: acta.Estado,
            TipoActa: acta.TipoActa,
            ActaOrigenId: acta.ActaOrigenId,
            ActaOrigenNumero: acta.ActaOrigen?.NumeroActa,
            ActaEntregada: acta.ActaEntregada,
            FechaCreacion: acta.FechaCreacion,
            FechaFirma: acta.FechaFirma,
            FechaFinalizacion: acta.FechaFinalizacion,
            FechaAnulacion: acta.FechaAnulacion,
            UsuarioId: acta.UsuarioId,
            DestinatarioUsuarioId: acta.DestinatarioUsuarioId,
            DestinatarioNombre: acta.DestinatarioUsuario != null ? $"{acta.DestinatarioUsuario.Nombre} {acta.DestinatarioUsuario.Apellido}" : null,
            DestinatarioCorreo: acta.DestinatarioUsuario?.Correo,
            DestinatarioCargo: destinatarioCargo ?? acta.DestinatarioUsuario?.Proceso,
            MensajeroUsuarioId: acta.MensajeroUsuarioId,
            MensajeroNombre: acta.MensajeroUsuario != null ? $"{acta.MensajeroUsuario.Nombre} {acta.MensajeroUsuario.Apellido}" : null,
            MensajeroCorreo: acta.MensajeroUsuario?.Correo,
            DeviceId: acta.DeviceId,
            FormDataJson: acta.FormDataJson ?? "{}",
            SignatureDataJson: acta.SignatureDataJson,
            HashSha256: acta.HashSha256,
            MotivoAnulacion: acta.MotivoAnulacion,
            Equipo: equipoResponse
        );
    }
}
