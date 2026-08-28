using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlataformaActas.Api.Modules.Equipment.Data.Entities
{
    public class Equipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string InventoryKey { get; set; } = string.Empty;
        public string? Empresa { get; set; }
        public string? AsignadoA { get; set; }
        public bool? ActaEntregada { get; set; }
        public string? Proveedor { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public string? HojaDeVida { get; set; }
        public string? Marca { get; set; }
        public string? Procesador { get; set; }
        public string? DiscoDuro { get; set; }
        public string? MemoriaRam { get; set; }
        public string? Descripcion { get; set; }
        public string? Departamento { get; set; }
        public string? EstatusEquipo { get; set; } = "Activo";
        public bool? MantenimientoRealizado { get; set; }
        public string? TipoMantenimiento { get; set; }
        public DateOnly? FechaMantenimiento { get; set; }
        public string? Observaciones { get; set; }
        public string SourceSystem { get; set; } = "P8-FR-019";
        public DateTimeOffset? LastSyncAt { get; set; }
    }
}

namespace PlataformaActas.Api.Modules.Equipment.Data.Configurations
{
    using PlataformaActas.Api.Modules.Equipment.Data.Entities;

    public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
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
}

namespace PlataformaActas.Api.Modules.Equipment.Models.Dto
{
    public sealed record EquipmentResponse(
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

    public sealed record CreateEquipmentRequest(
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

    public sealed record UpdateEquipmentRequest(
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

    public sealed record EquipmentMaintenanceItem(
        Guid Id,
        string InventoryKey,
        string NombreEquipo,
        string? Marca,
        string? AsignadoA,
        string? Departamento,
        string? EstatusEquipo,
        string? TipoMantenimiento,
        DateOnly? FechaMantenimiento,
        bool MantenimientoRealizado
    );

    public sealed record UpdateMaintenanceRequest(
        string? TipoMantenimiento,
        DateOnly? FechaMantenimiento,
        bool MantenimientoRealizado
    );
}

namespace PlataformaActas.Api.Modules.Equipment.Models.Mappers
{
    using PlataformaActas.Api.Modules.Equipment.Data.Entities;
    using PlataformaActas.Api.Modules.Equipment.Models.Dto;

    public static class EquipmentMappingExtensions
    {
        public static EquipmentResponse ToResponse(this Equipment equipment)
        {
            return new EquipmentResponse(
                equipment.Id,
                equipment.InventoryKey,
                equipment.NombreEquipo,
                equipment.Empresa,
                equipment.AsignadoA,
                equipment.Proveedor,
                equipment.Marca,
                equipment.Procesador,
                equipment.DiscoDuro,
                equipment.MemoriaRam,
                equipment.Departamento,
                equipment.EstatusEquipo
            );
        }
    }
}
