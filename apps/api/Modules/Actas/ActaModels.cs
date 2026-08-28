using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipment.Data.Entities.Equipment;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Entities
{
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

        public string? Categoria { get; set; }
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
        public string? DeviceId { get; set; }
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
}

namespace PlataformaActas.Api.Modules.Actas.Data.Configurations
{
    using PlataformaActas.Api.Modules.Actas.Data.Entities;

    public sealed class ActaConfiguration : IEntityTypeConfiguration<Acta>
    {
        public void Configure(EntityTypeBuilder<Acta> builder)
        {
            builder.ToTable("actas_v2");

            builder.HasKey(a => a.Id)
                .HasName("actas_v2_pkey");

            builder.Property(a => a.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(a => a.NumeroActa)
                .HasColumnName("numero_acta")
                .HasMaxLength(50);

            builder.HasIndex(a => a.NumeroActa)
                .IsUnique()
                .HasDatabaseName("ix_actas_v2_numero_acta");

            builder.Property(a => a.EquipmentId)
                .HasColumnName("equipment_id")
                .IsRequired();

            builder.HasIndex(a => a.EquipmentId)
                .HasDatabaseName("ix_actas_v2_equipment_id");

            builder.Property(a => a.UsuarioId)
                .HasColumnName("usuario_id")
                .IsRequired();

            builder.HasIndex(a => a.UsuarioId)
                .HasDatabaseName("ix_actas_v2_usuario_id");

            builder.Property(a => a.DestinatarioUsuarioId)
                .HasColumnName("destinatario_usuario_id");

            builder.HasIndex(a => a.DestinatarioUsuarioId)
                .HasDatabaseName("ix_actas_v2_destinatario_usuario_id");

            builder.Property(a => a.MensajeroUsuarioId)
                .HasColumnName("mensajero_usuario_id");

            builder.HasIndex(a => a.MensajeroUsuarioId)
                .HasDatabaseName("ix_actas_v2_mensajero_usuario_id");

            builder.Property(a => a.ActaEntregada)
                .HasColumnName("acta_entregada")
                .HasDefaultValue(false);

            builder.Property(a => a.Estado)
                .HasColumnName("estado")
                .HasMaxLength(50)
                .HasDefaultValue("Borrador")
                .IsRequired();

            builder.HasIndex(a => a.Estado)
                .HasDatabaseName("ix_actas_v2_estado");

            builder.Property(a => a.TipoActa)
                .HasColumnName("tipo_acta")
                .HasMaxLength(50)
                .HasDefaultValue("Entrega")
                .IsRequired();

            builder.Property(a => a.ActaOrigenId)
                .HasColumnName("acta_origen_id");

            builder.Property(a => a.RutaDocumento)
                .HasColumnName("ruta_documento");

            builder.Property(a => a.FechaFirma)
                .HasColumnName("fecha_firma");

            builder.Property(a => a.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.Property(a => a.FechaAnulacion)
                .HasColumnName("fecha_anulacion");

            builder.Property(a => a.FechaFinalizacion)
                .HasColumnName("fecha_finalizacion");

            builder.Property(a => a.DeviceId)
                .HasColumnName("device_id")
                .HasMaxLength(100);

            builder.Property(a => a.FormDataJson)
                .HasColumnName("form_data_json")
                .HasColumnType("jsonb")
                .HasDefaultValue("{}")
                .IsRequired();

            builder.Property(a => a.HashSha256)
                .HasColumnName("hash_sha256")
                .HasMaxLength(64);

            builder.Property(a => a.MotivoAnulacion)
                .HasColumnName("motivo_anulacion");

            builder.Property(a => a.SignatureDataJson)
                .HasColumnName("signature_data_json")
                .HasColumnType("jsonb");

            builder.HasOne(a => a.Equipment)
                .WithMany()
                .HasForeignKey(a => a.EquipmentId)
                .HasConstraintName("fk_actas_v2_equipment")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .HasConstraintName("fk_actas_v2_usuario")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.DestinatarioUsuario)
                .WithMany()
                .HasForeignKey(a => a.DestinatarioUsuarioId)
                .HasConstraintName("fk_actas_v2_destinatario")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.MensajeroUsuario)
                .WithMany()
                .HasForeignKey(a => a.MensajeroUsuarioId)
                .HasConstraintName("fk_actas_v2_mensajero")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ActaOrigen)
                .WithMany()
                .HasForeignKey(a => a.ActaOrigenId)
                .HasConstraintName("fk_actas_v2_acta_origen")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public sealed class ActaObservationConfiguration : IEntityTypeConfiguration<ActaObservation>
    {
        public void Configure(EntityTypeBuilder<ActaObservation> builder)
        {
            builder.ToTable("acta_observations_v2");

            builder.HasKey(o => o.Id)
                .HasName("acta_observations_v2_pkey");

            builder.Property(o => o.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(o => o.ActaId)
                .HasColumnName("acta_id")
                .IsRequired();

            builder.HasIndex(o => o.ActaId)
                .HasDatabaseName("ix_acta_observations_v2_acta_id");

            builder.Property(o => o.Categoria)
                .HasColumnName("categoria")
                .HasMaxLength(100);

            builder.Property(o => o.Texto)
                .HasColumnName("texto")
                .IsRequired();

            builder.Property(o => o.UsuarioId)
                .HasColumnName("usuario_id");

            builder.HasIndex(o => o.UsuarioId)
                .HasDatabaseName("ix_acta_observations_v2_usuario_id");

            builder.Property(o => o.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.HasOne(o => o.Acta)
                .WithMany(a => a.Observations)
                .HasForeignKey(o => o.ActaId)
                .HasConstraintName("fk_acta_observations_v2_acta")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.Usuario)
                .WithMany()
                .HasForeignKey(o => o.UsuarioId)
                .HasConstraintName("fk_acta_observations_v2_usuario")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public sealed class ActaSignatureConfiguration : IEntityTypeConfiguration<ActaSignature>
    {
        public void Configure(EntityTypeBuilder<ActaSignature> builder)
        {
            builder.ToTable("acta_signatures_v2");

            builder.HasKey(s => s.Id)
                .HasName("acta_signatures_v2_pkey");

            builder.Property(s => s.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(s => s.ActaId)
                .HasColumnName("acta_id")
                .IsRequired();

            builder.HasIndex(s => s.ActaId)
                .HasDatabaseName("ix_acta_signatures_v2_acta_id");

            builder.Property(s => s.SignatureBase64)
                .HasColumnName("signature_base64")
                .IsRequired();

            builder.Property(s => s.DeviceId)
                .HasColumnName("device_id")
                .HasMaxLength(100);

            builder.Property(s => s.ConsentAccepted)
                .HasColumnName("consent_accepted")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(s => s.LegalClauseVersion)
                .HasColumnName("legal_clause_version")
                .HasMaxLength(50);

            builder.Property(s => s.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.HasOne(s => s.Acta)
                .WithMany(a => a.Signatures)
                .HasForeignKey(s => s.ActaId)
                .HasConstraintName("fk_acta_signatures_v2_acta")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class ActaStatusHistoryConfiguration : IEntityTypeConfiguration<ActaStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ActaStatusHistory> builder)
        {
            builder.ToTable("acta_status_history_v2");

            builder.HasKey(h => h.Id)
                .HasName("acta_status_history_v2_pkey");

            builder.Property(h => h.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(h => h.ActaId)
                .HasColumnName("acta_id")
                .IsRequired();

            builder.HasIndex(h => h.ActaId)
                .HasDatabaseName("ix_acta_status_history_acta_id");

            builder.Property(h => h.EstadoAnterior)
                .HasColumnName("estado_anterior")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(h => h.EstadoNuevo)
                .HasColumnName("estado_nuevo")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(h => h.UsuarioId)
                .HasColumnName("usuario_id");

            builder.HasIndex(h => h.UsuarioId)
                .HasDatabaseName("ix_acta_status_history_usuario_id");

            builder.Property(h => h.Comentario)
                .HasColumnName("comentario")
                .HasMaxLength(1000);

            builder.Property(h => h.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.HasOne(h => h.Acta)
                .WithMany(a => a.StatusHistories)
                .HasForeignKey(h => h.ActaId)
                .HasConstraintName("fk_acta_status_history_acta")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.Usuario)
                .WithMany()
                .HasForeignKey(h => h.UsuarioId)
                .HasConstraintName("fk_acta_status_history_usuario")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

namespace PlataformaActas.Api.Modules.Actas.Models.Dto
{
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
        string SignatureDataJson
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
}

namespace PlataformaActas.Api.Modules.Actas.Models.Mappers
{
    using PlataformaActas.Api.Modules.Actas.Data.Entities;
    using PlataformaActas.Api.Modules.Actas.Models.Dto;

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
}
