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

        builder.HasIndex(a => a.FechaCreacion)
            .HasDatabaseName("ix_actas_v2_fecha_creacion");

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
