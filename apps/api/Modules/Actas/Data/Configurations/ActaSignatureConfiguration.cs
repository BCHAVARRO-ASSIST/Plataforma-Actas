using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Configurations;

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
            .IsRequired();

        builder.Property(s => s.LegalClauseVersion)
            .HasColumnName("legal_clause_version")
            .HasMaxLength(50);

        builder.Property(s => s.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasDefaultValueSql("NOW()")
            .IsRequired();
    }
}
