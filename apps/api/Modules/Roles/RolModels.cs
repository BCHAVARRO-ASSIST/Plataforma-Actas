using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlataformaActas.Api.Modules.Roles.Data.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }

    public class Permiso
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Modulo { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Codigo
        {
            get => Nombre;
            set { }
        }

        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }

    public class RolPermiso
    {
        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public int PermisoId { get; set; }
        public Permiso Permiso { get; set; } = null!;
    }

    public class UsuarioRol
    {
        public int UsuarioId { get; set; }
        public PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario Usuario { get; set; } = null!;

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    }
}

namespace PlataformaActas.Api.Modules.Roles.Data.Configurations
{
    using PlataformaActas.Api.Modules.Roles.Data.Entities;

    public class RolConfiguration : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(r => r.Nombre)
                .IsUnique();

            builder.Property(r => r.Descripcion)
                .HasMaxLength(250);

            builder.Property(r => r.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(r => r.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }

    public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
    {
        public void Configure(EntityTypeBuilder<Permiso> builder)
        {
            builder.ToTable("permisos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(p => p.Nombre)
                .IsUnique();

            builder.Property(p => p.Descripcion)
                .HasMaxLength(250);

            builder.Property(p => p.Modulo)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Activo)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }

    public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
    {
        public void Configure(EntityTypeBuilder<RolPermiso> builder)
        {
            builder.ToTable("rol_permisos");

            builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

            builder.HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.PermisoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
    {
        public void Configure(EntityTypeBuilder<UsuarioRol> builder)
        {
            builder.ToTable("usuario_roles");

            builder.HasKey(ur => new { ur.UsuarioId, ur.RolId });

            builder.Property(ur => ur.FechaAsignacion)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

namespace PlataformaActas.Api.Modules.Roles.Models.Dto
{
    public enum EliminarRolResultado
    {
        Eliminado,
        NoEncontrado,
        TieneUsuarios,
        AdministradorProtegido
    }

    public record RolResponse(int Id, string Nombre, string? Descripcion, bool Activo, DateTime FechaCreacion);
    public record RolDetalleResponse(int Id, string Nombre, string? Descripcion, bool Activo, DateTime FechaCreacion, IReadOnlyList<PermisoResponse> Permisos);
    public record PermisoResponse(int Id, string Codigo, string Nombre, string? Descripcion, string Modulo);
    public record CreateRolRequest(string Nombre, string? Descripcion);
    public record UpdateRolRequest(string Nombre, string? Descripcion);
    public record CrearRolRequest(string Nombre, string? Descripcion);
    public record ActualizarRolRequest(string Nombre, string? Descripcion);
    public record ActualizarPermisosRolRequest(IReadOnlyList<int> PermisoIds);
    public record UpdateRolPermisosRequest(IReadOnlyList<int> PermisoIds);
    public record SetRolEstadoRequest(bool Activo);
}

namespace PlataformaActas.Api.Modules.Roles.Models.Mappers
{
    using PlataformaActas.Api.Modules.Roles.Data.Entities;
    using PlataformaActas.Api.Modules.Roles.Models.Dto;

    public static class RolMappingExtensions
    {
        public static RolResponse ToResponse(this Rol rol)
        {
            return new RolResponse(rol.Id, rol.Nombre, rol.Descripcion, rol.Activo, rol.FechaCreacion);
        }

        public static RolDetalleResponse ToDetalleResponse(this Rol rol)
        {
            var permisos = rol.RolPermisos
                .Select(rp => rp.Permiso.ToResponse())
                .ToList();

            return new RolDetalleResponse(rol.Id, rol.Nombre, rol.Descripcion, rol.Activo, rol.FechaCreacion, permisos);
        }

        public static PermisoResponse ToResponse(this Permiso permiso)
        {
            var codigo = string.IsNullOrWhiteSpace(permiso.Codigo) ? permiso.Nombre : permiso.Codigo;
            return new PermisoResponse(permiso.Id, codigo, permiso.Nombre, permiso.Descripcion, permiso.Modulo);
        }
    }
}
