using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Infrastructure.Persistence;

public class PlataformaActasDbContext : DbContext
{
    public PlataformaActasDbContext(DbContextOptions<PlataformaActasDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario> Usuarios => Set<PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario>();
    public DbSet<PlataformaActas.Api.Modules.Auth.Data.Entities.PasswordResetToken> PasswordResetTokens => Set<PlataformaActas.Api.Modules.Auth.Data.Entities.PasswordResetToken>();
    public DbSet<PlataformaActas.Api.Modules.Equipos.Data.Entities.Equipment> Equipment => Set<PlataformaActas.Api.Modules.Equipos.Data.Entities.Equipment>();
    public DbSet<PlataformaActas.Api.Modules.Actas.Data.Entities.Acta> Actas => Set<PlataformaActas.Api.Modules.Actas.Data.Entities.Acta>();
    public DbSet<PlataformaActas.Api.Modules.Actas.Data.Entities.ActaObservation> ActaObservations => Set<PlataformaActas.Api.Modules.Actas.Data.Entities.ActaObservation>();
    public DbSet<PlataformaActas.Api.Modules.Actas.Data.Entities.ActaSignature> ActaSignatures => Set<PlataformaActas.Api.Modules.Actas.Data.Entities.ActaSignature>();
    public DbSet<PlataformaActas.Api.Modules.Actas.Data.Entities.ActaStatusHistory> ActaStatusHistories => Set<PlataformaActas.Api.Modules.Actas.Data.Entities.ActaStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlataformaActasDbContext).Assembly);
    }
}

