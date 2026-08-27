using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentEntity = PlataformaActas.Api.Modules.Equipment.Data.Entities.Equipment;

namespace PlataformaActas.Api.Modules.Equipment.Data.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<EquipmentEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentEntity> builder)
    {
        builder.ToTable("equipment");

        builder.HasKey(e => e.Id)
            .HasName("equipment_pkey");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.InventoryKey)
            .HasColumnName("inventory_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.InventoryKey)
            .IsUnique()
            .HasDatabaseName("equipment_inventory_key_key");

        builder.Property(e => e.Empresa)
            .HasColumnName("empresa")
            .HasMaxLength(150);

        builder.Property(e => e.AsignadoA)
            .HasColumnName("asignado_a")
            .HasMaxLength(200);

        builder.Property(e => e.ActaEntregada)
            .HasColumnName("acta_entregada");

        builder.Property(e => e.Proveedor)
            .HasColumnName("proveedor")
            .HasMaxLength(150);

        builder.Property(e => e.NombreEquipo)
            .HasColumnName("nombre_equipo")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(e => e.NombreEquipo)
            .HasDatabaseName("ix_equipment_nombre");

        builder.Property(e => e.HojaDeVida)
            .HasColumnName("hoja_de_vida");

        builder.Property(e => e.Marca)
            .HasColumnName("marca")
            .HasMaxLength(100);

        builder.Property(e => e.Procesador)
            .HasColumnName("procesador")
            .HasMaxLength(200);

        builder.Property(e => e.DiscoDuro)
            .HasColumnName("disco_duro")
            .HasMaxLength(150);

        builder.Property(e => e.MemoriaRam)
            .HasColumnName("memoria_ram")
            .HasMaxLength(100);

        builder.Property(e => e.Descripcion)
            .HasColumnName("descripcion");

        builder.Property(e => e.Departamento)
            .HasColumnName("departamento")
            .HasMaxLength(150);

        builder.Property(e => e.EstatusEquipo)
            .HasColumnName("estatus_equipo")
            .HasMaxLength(100);

        builder.HasIndex(e => e.EstatusEquipo)
            .HasDatabaseName("ix_equipment_estado");

        builder.Property(e => e.MantenimientoRealizado)
            .HasColumnName("mantenimiento_realizado");

        builder.Property(e => e.TipoMantenimiento)
            .HasColumnName("tipo_mantenimiento")
            .HasMaxLength(150);

        builder.Property(e => e.FechaMantenimiento)
            .HasColumnName("fecha_mantenimiento");

        builder.Property(e => e.Observaciones)
            .HasColumnName("observaciones");

        builder.Property(e => e.SourceSystem)
            .HasColumnName("source_system")
            .HasMaxLength(50)
            .HasDefaultValue("P8-FR-019")
            .IsRequired();

        builder.Property(e => e.LastSyncAt)
            .HasColumnName("last_sync_at");
    }
}
