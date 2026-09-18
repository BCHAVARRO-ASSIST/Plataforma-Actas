using System;
using System.Collections.Generic;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipos.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Entities;

public class Acta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? NumeroActa { get; set; }

    public Guid EquipmentId { get; set; }
    public EquipmentEntity Equipment { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int? DestinatarioUsuarioId { get; set; }
    public Usuario? DestinatarioUsuario { get; set; }

    public int? MensajeroUsuarioId { get; set; }
    public Usuario? MensajeroUsuario { get; set; }

    public bool ActaEntregada { get; set; }
    public string Estado { get; set; } = "Borrador";
    public string TipoActa { get; set; } = "Entrega";

    public Guid? ActaOrigenId { get; set; }
    public Acta? ActaOrigen { get; set; }

    public string? RutaDocumento { get; set; }
    public DateTimeOffset? FechaFirma { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaAnulacion { get; set; }
    public DateTimeOffset? FechaFinalizacion { get; set; }
    public string? DeviceId { get; set; }

    public string FormDataJson { get; set; } = "{}";
    public string? HashSha256 { get; set; }
    public string? MotivoAnulacion { get; set; }
    public string? SignatureDataJson { get; set; }

    public ICollection<ActaObservation> Observations { get; set; } = new List<ActaObservation>();
    public ICollection<ActaSignature> Signatures { get; set; } = new List<ActaSignature>();
    public ICollection<ActaStatusHistory> StatusHistories { get; set; } = new List<ActaStatusHistory>();
}

public class ActaObservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActaId { get; set; }
    public Acta Acta { get; set; } = null!;

    public string Texto { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}

public class ActaSignature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActaId { get; set; }
    public Acta Acta { get; set; } = null!;

    public string SignatureBase64 { get; set; } = string.Empty;
    public bool ConsentAccepted { get; set; } = true;
    public string? LegalClauseVersion { get; set; } = "3.2-ISO27001";
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}

public class ActaStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActaId { get; set; }
    public Acta Acta { get; set; } = null!;

    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string? Comentario { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}
