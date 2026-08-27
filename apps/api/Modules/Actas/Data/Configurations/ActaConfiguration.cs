using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Configurations;

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
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.Estado)
            .HasColumnName("estado")
            .HasMaxLength(50)
            .HasDefaultValue("Borrador")
            .IsRequired();

        builder.HasIndex(a => a.Estado)
            .HasDatabaseName("ix_actas_v2_estado");

        builder.Property(a => a.TipoActa)
            .HasColumnName("tipo_acta")
            .HasMaxLength(20)
            .HasDefaultValue("Entrega")
            .IsRequired();

        builder.HasIndex(a => a.TipoActa)
            .HasDatabaseName("ix_actas_v2_tipo_acta");

        builder.Property(a => a.ActaOrigenId)
            .HasColumnName("acta_origen_id");

        builder.HasIndex(a => a.ActaOrigenId)
            .HasDatabaseName("ix_actas_v2_acta_origen_id");

        builder.Property(a => a.RutaDocumento)
            .HasColumnName("ruta_documento")
            .HasMaxLength(500);

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
            .HasMaxLength(200);

        builder.Property(a => a.FormDataJson)
            .HasColumnName("form_data_json")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(a => a.HashSha256)
            .HasColumnName("hash_sha256")
            .HasMaxLength(128);

        builder.Property(a => a.MotivoAnulacion)
            .HasColumnName("motivo_anulacion")
            .HasMaxLength(2000);

        builder.Property(a => a.SignatureDataJson)
            .HasColumnName("signature_data_json")
            .HasColumnType("jsonb");

        // Relationships
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

        builder.HasMany(a => a.Observations)
            .WithOne(o => o.Acta)
            .HasForeignKey(o => o.ActaId)
            .HasConstraintName("fk_acta_observations_v2_acta")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Signatures)
            .WithOne(s => s.Acta)
            .HasForeignKey(s => s.ActaId)
            .HasConstraintName("fk_acta_signatures_v2_acta")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.StatusHistories)
            .WithOne(h => h.Acta)
            .HasForeignKey(h => h.ActaId)
            .HasConstraintName("fk_acta_status_history_v2_acta")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
