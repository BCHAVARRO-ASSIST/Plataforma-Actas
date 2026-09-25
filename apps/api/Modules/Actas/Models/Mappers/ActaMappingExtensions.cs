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
            InventoryKey: acta.Equipo?.InventoryKey,
            NombreEquipo: acta.Equipo?.NombreEquipo,
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
            MotivoAnulacion: acta.MotivoAnulacion,
            ObservacionesEntrega: acta.ObservacionesEntrega,
            ObservacionesDevolucion: acta.ObservacionesDevolucion
        );
    }

    public static ActaDetailResponse ToDetailResponse(this Acta acta)
    {
        var equipoResponse = acta.Equipo != null
            ? new EquipmentActaResponse(
                Id: acta.Equipo.Id,
                InventoryKey: acta.Equipo.InventoryKey,
                NombreEquipo: acta.Equipo.NombreEquipo,
                Empresa: acta.Equipo.Empresa,
                AsignadoA: acta.Equipo.AsignadoA,
                Proveedor: acta.Equipo.Proveedor,
                Marca: acta.Equipo.Marca,
                Procesador: acta.Equipo.Procesador,
                DiscoDuro: acta.Equipo.DiscoDuro,
                MemoriaRam: acta.Equipo.MemoriaRam,
                Departamento: acta.Equipo.Departamento,
                EstatusEquipo: acta.Equipo.EstatusEquipo
            )
            : new EquipmentActaResponse(
                Guid.Empty, string.Empty, string.Empty, null, null, null, null, null, null, null, null, null
            );

        string? destinatarioCargo = null;
        string? destinatarioDocumento = null;
        if (!string.IsNullOrWhiteSpace(acta.FormDataJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(acta.FormDataJson);
                if (doc.RootElement.TryGetProperty("destinatarioCargo", out var cargoElem))
                {
                    destinatarioCargo = cargoElem.GetString();
                }

                // Prioridad 1: Valor manual guardado en el acta específica
                if (doc.RootElement.TryGetProperty("numeroDocumento", out var ndElem) && ndElem.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(ndElem.GetString()))
                {
                    destinatarioDocumento = ndElem.GetString()?.Trim();
                }
                else if (doc.RootElement.TryGetProperty("cedula", out var cElem) && cElem.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(cElem.GetString()))
                {
                    destinatarioDocumento = cElem.GetString()?.Trim();
                }
                else if (doc.RootElement.TryGetProperty("documento", out var dElem) && dElem.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(dElem.GetString()))
                {
                    destinatarioDocumento = dElem.GetString()?.Trim();
                }
            }
            catch
            {
                // Ignorar error de deserialización
            }
        }

        // Prioridad 2: Perfil de usuario registrado
        if (string.IsNullOrWhiteSpace(destinatarioDocumento))
        {
            destinatarioDocumento = acta.DestinatarioUsuario?.NumeroDocumento?.Trim();
        }

        // Prioridad 3: Fallback a "—"
        if (string.IsNullOrWhiteSpace(destinatarioDocumento))
        {
            destinatarioDocumento = "—";
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
            DestinatarioDocumento: destinatarioDocumento,
            MensajeroUsuarioId: acta.MensajeroUsuarioId,
            MensajeroNombre: acta.MensajeroUsuario != null ? $"{acta.MensajeroUsuario.Nombre} {acta.MensajeroUsuario.Apellido}" : null,
            MensajeroCorreo: acta.MensajeroUsuario?.Correo,
            DeviceId: acta.DeviceId,
            FormDataJson: acta.FormDataJson ?? "{}",
            SignatureDataJson: acta.SignatureDataJson,
            HashSha256: acta.HashSha256,
            MotivoAnulacion: acta.MotivoAnulacion,
            Equipo: equipoResponse,
            ObservacionesEntrega: acta.ObservacionesEntrega,
            ObservacionesDevolucion: acta.ObservacionesDevolucion
        );
    }
}
