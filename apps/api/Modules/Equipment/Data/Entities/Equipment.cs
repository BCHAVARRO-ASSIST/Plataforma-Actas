namespace PlataformaActas.Api.Modules.Equipment.Data.Entities;

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
