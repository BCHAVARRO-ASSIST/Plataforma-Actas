using System.Data;
using ExcelDataReader;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Equipos.Data.Entities;

namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed class GoogleDriveSyncService : IGoogleDriveSyncService
{
    private readonly GoogleIntegrationOptions _options;
    private readonly IHostEnvironment _env;
    private readonly PlataformaActasDbContext _context;
    private readonly ILogger<GoogleDriveSyncService> _logger;
    private static SyncResult? _lastSyncResult;

    public GoogleDriveSyncService(
        IOptions<GoogleIntegrationOptions> options,
        IHostEnvironment env,
        PlataformaActasDbContext context,
        ILogger<GoogleDriveSyncService> logger)
    {
        _options = options.Value;
        _env = env;
        _context = context;
        _logger = logger;
    }

    public SyncResult? GetLastSyncStatus() => _lastSyncResult;

    public async Task<SyncResult> SyncInventoryAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new SyncResult(false, "La sincronización con Google Drive está deshabilitada en la configuración.", 0, 0, 0, 0, DateTimeOffset.UtcNow);
        }

        var keyPath = ResolveKeyPath(_options.ServiceAccountKeyPath);
        if (!File.Exists(keyPath))
        {
            _logger.LogError("Archivo de credenciales de Google no encontrado en: {KeyPath}", keyPath);
            return new SyncResult(false, $"Archivo de credenciales no encontrado en: {keyPath}", 0, 0, 0, 0, DateTimeOffset.UtcNow);
        }

        try
        {
            _logger.LogInformation("Iniciando sincronización con Google Drive para archivo: {FileId}...", _options.DriveFileId);

            var credential = GoogleCredential.FromFile(keyPath)
                .CreateScoped(DriveService.ScopeConstants.DriveReadonly)
                .CreateWithUser(_options.AdminImpersonationEmail);

            using var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PlataformaActas"
            });

            using var memoryStream = new MemoryStream();

            // 1. Obtener metadatos del archivo para determinar si es Google Spreadsheet nativo o archivo binario
            var metaRequest = driveService.Files.Get(_options.DriveFileId);
            metaRequest.SupportsAllDrives = true;
            metaRequest.Fields = "id, name, mimeType, size";
            var fileMetadata = await metaRequest.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Archivo detectado en Google Drive: '{Name}' (MimeType: {MimeType})", fileMetadata.Name, fileMetadata.MimeType);

            if (string.Equals(fileMetadata.MimeType, "application/vnd.google-apps.spreadsheet", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Exportando Google Spreadsheet nativo '{Name}' a formato OpenXML (.xlsx)...", fileMetadata.Name);
                var exportRequest = driveService.Files.Export(_options.DriveFileId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                await exportRequest.DownloadAsync(memoryStream, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Descargando archivo binario directo '{Name}' desde Google Drive...", fileMetadata.Name);
                var getRequest = driveService.Files.Get(_options.DriveFileId);
                getRequest.SupportsAllDrives = true;
                await getRequest.DownloadAsync(memoryStream, cancellationToken);
            }

            memoryStream.Position = 0;

            if (memoryStream.Length == 0)
            {
                throw new InvalidOperationException("El archivo descargado desde Google Drive está vacío.");
            }

            _logger.LogInformation("Archivo descargado/exportado exitosamente ({Size} bytes). Procesando Excel...", memoryStream.Length);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(memoryStream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
            });

            var targetSheet = dataSet.Tables
                .Cast<DataTable>()
                .FirstOrDefault(t => t.TableName.Equals(_options.SheetName, StringComparison.OrdinalIgnoreCase));

            if (targetSheet == null)
            {
                var available = string.Join(", ", dataSet.Tables.Cast<DataTable>().Select(t => $"'{t.TableName}'"));
                throw new InvalidOperationException($"La pestaña '{_options.SheetName}' no fue encontrada en el Excel. Pestañas disponibles: {available}");
            }

            var result = await ProcessSheetAsync(targetSheet, cancellationToken);
            _lastSyncResult = result;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la sincronización del inventario desde Google Drive.");
            var failedResult = new SyncResult(false, $"Error al sincronizar: {ex.Message}", 0, 0, 0, 0, DateTimeOffset.UtcNow);
            _lastSyncResult = failedResult;
            return failedResult;
        }
    }

    private async Task<SyncResult> ProcessSheetAsync(DataTable table, CancellationToken cancellationToken)
    {
        var headerRowIndex = _options.HeaderRowIndex - 1; // 0-based
        if (table.Rows.Count <= headerRowIndex)
        {
            throw new InvalidOperationException($"La hoja '{table.TableName}' tiene solo {table.Rows.Count} filas, no contiene la fila de encabezados esperada ({_options.HeaderRowIndex}).");
        }

        var headerRow = table.Rows[headerRowIndex];
        var headersList = string.Join(" | ", headerRow.ItemArray.Select((item, idx) => $"[{idx}]={item?.ToString()?.Trim()}"));
        _logger.LogInformation("Encabezados encontrados en Fila 6 del Excel: {Headers}", headersList);

        var colMap = BuildColumnMap(headerRow);
        _logger.LogInformation("Mapeo de columnas procesado: {Map}", string.Join(", ", colMap.Select(kv => $"{kv.Key}->Col{kv.Value}")));

        if (!colMap.ContainsKey("inventory_key"))
        {
            throw new InvalidOperationException("No se encontró la columna obligatoria 'Nombre del Equipo' / 'Inventory Key' en los encabezados del Excel.");
        }

        int totalProcessed = 0;
        int createdCount = 0;
        int updatedCount = 0;
        int prunedCount = 0;
        var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = headerRowIndex + 1; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var inventoryKey = GetValue(row, colMap, "inventory_key");

            if (string.IsNullOrWhiteSpace(inventoryKey))
            {
                continue; // Fila vacía
            }

            totalProcessed++;
            var normalizedKey = inventoryKey.Trim();
            processedKeys.Add(normalizedKey);

            var existing = await _context.Equipos
                .FirstOrDefaultAsync(e => e.InventoryKey.ToLower() == normalizedKey.ToLower(), cancellationToken);

            var empresa = GetValue(row, colMap, "empresa");
            var asignadoA = GetValue(row, colMap, "asignado_a");
            var actaEntregadaStr = GetValue(row, colMap, "acta_entregada");
            var actaEntregada = ParseBool(actaEntregadaStr);
            var proveedor = GetValue(row, colMap, "proveedor");
            var descripcion = GetValue(row, colMap, "descripcion");
            var nombreEquipo = !string.IsNullOrWhiteSpace(descripcion) ? descripcion : normalizedKey;
            var hojaDeVida = GetValue(row, colMap, "hoja_de_vida");
            var marca = GetValue(row, colMap, "marca");
            var procesador = GetValue(row, colMap, "procesador");
            var discoDuro = GetValue(row, colMap, "disco_duro");
            var memoriaRam = GetValue(row, colMap, "memoria_ram");
            var departamento = GetValue(row, colMap, "departamento");
            var estatus = GetValue(row, colMap, "estatus_equipo");
            if (string.IsNullOrWhiteSpace(estatus)) estatus = "Activo";

            var mantRealizadoStr = GetValue(row, colMap, "mantenimiento_realizado");
            var mantRealizado = ParseBool(mantRealizadoStr);
            var tipoMant = GetValue(row, colMap, "tipo_mantenimiento");
            var fechaMant = ParseDate(GetValue(row, colMap, "fecha_mantenimiento"));

            if (existing != null)
            {
                // Actualizar
                if (!string.IsNullOrWhiteSpace(empresa)) existing.Empresa = empresa;
                if (!string.IsNullOrWhiteSpace(asignadoA)) existing.AsignadoA = asignadoA;
                if (actaEntregada.HasValue) existing.ActaEntregada = actaEntregada.Value;
                if (!string.IsNullOrWhiteSpace(proveedor)) existing.Proveedor = proveedor;
                existing.NombreEquipo = nombreEquipo;
                if (!string.IsNullOrWhiteSpace(hojaDeVida)) existing.HojaDeVida = hojaDeVida;
                if (!string.IsNullOrWhiteSpace(marca)) existing.Marca = marca;
                if (!string.IsNullOrWhiteSpace(procesador)) existing.Procesador = procesador;
                if (!string.IsNullOrWhiteSpace(discoDuro)) existing.DiscoDuro = discoDuro;
                if (!string.IsNullOrWhiteSpace(memoriaRam)) existing.MemoriaRam = memoriaRam;
                if (!string.IsNullOrWhiteSpace(descripcion)) existing.Descripcion = descripcion;
                if (!string.IsNullOrWhiteSpace(departamento)) existing.Departamento = departamento;
                if (!string.IsNullOrWhiteSpace(estatus)) existing.EstatusEquipo = estatus;
                if (mantRealizado.HasValue) existing.MantenimientoRealizado = mantRealizado.Value;
                if (!string.IsNullOrWhiteSpace(tipoMant)) existing.TipoMantenimiento = tipoMant;
                if (fechaMant.HasValue) existing.FechaMantenimiento = fechaMant.Value;

                existing.LastSyncAt = DateTimeOffset.UtcNow;
                updatedCount++;
            }
            else
            {
                // Crear nuevo
                var newEquipment = new Equipo
                {
                    Id = Guid.NewGuid(),
                    InventoryKey = normalizedKey,
                    NombreEquipo = nombreEquipo,
                    Empresa = empresa,
                    AsignadoA = asignadoA,
                    ActaEntregada = actaEntregada,
                    Proveedor = proveedor,
                    HojaDeVida = hojaDeVida,
                    Marca = marca,
                    Procesador = procesador,
                    DiscoDuro = discoDuro,
                    MemoriaRam = memoriaRam,
                    Descripcion = descripcion,
                    Departamento = departamento,
                    EstatusEquipo = estatus,
                    MantenimientoRealizado = mantRealizado,
                    TipoMantenimiento = tipoMant,
                    FechaMantenimiento = fechaMant,
                    SourceSystem = "P8-FR-019",
                    LastSyncAt = DateTimeOffset.UtcNow
                };

                _context.Equipos.Add(newEquipment);
                createdCount++;
            }
        }

        // Limpiar o inactivar equipos eliminados o renombrados en el Excel
        var allSyncEquipments = await _context.Equipos
            .Where(e => e.SourceSystem == "P8-FR-019")
            .ToListAsync(cancellationToken);

        foreach (var eq in allSyncEquipments)
        {
            if (!processedKeys.Contains(eq.InventoryKey))
            {
                var hasActas = await _context.Actas.AnyAsync(a => a.EquipmentId == eq.Id, cancellationToken);
                if (!hasActas)
                {
                    _context.Equipos.Remove(eq);
                    prunedCount++;
                }
                else if (eq.EstatusEquipo != "Inactivo")
                {
                    eq.EstatusEquipo = "Inactivo";
                    updatedCount++;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sincronización finalizada: {Total} leídos, {Created} creados, {Updated} actualizados, {Pruned} eliminados por cambio/baja en Excel.",
            totalProcessed, createdCount, updatedCount, prunedCount);

        return new SyncResult(
            Success: true,
            Message: $"Sincronización completada exitosamente. Total: {totalProcessed}, Creados: {createdCount}, Actualizados: {updatedCount}, Desvinculados: {prunedCount}.",
            TotalProcessed: totalProcessed,
            Created: createdCount,
            Updated: updatedCount,
            Unchanged: 0,
            Timestamp: DateTimeOffset.UtcNow
        );
    }

    private static Dictionary<string, int> BuildColumnMap(DataRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int c = 0; c < headerRow.ItemArray.Length; c++)
        {
            var header = headerRow[c]?.ToString()?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(header)) continue;

            if (header.Contains("empresa") && !map.ContainsKey("empresa")) map["empresa"] = c;
            else if (header.Contains("asignado") && !map.ContainsKey("asignado_a")) map["asignado_a"] = c;
            else if (header.Contains("acta") && !map.ContainsKey("acta_entregada")) map["acta_entregada"] = c;
            else if (header.Contains("proveedor") && !map.ContainsKey("proveedor")) map["proveedor"] = c;
            else if ((header.Contains("nombre del equipo") || header.Contains("nombre equipo") || header.Contains("equipo")) && !map.ContainsKey("inventory_key"))
            {
                map["inventory_key"] = c;
            }
            else if (header.Contains("hoja de vida") && !map.ContainsKey("hoja_de_vida")) map["hoja_de_vida"] = c;
            else if (header.Contains("marca") && !map.ContainsKey("marca")) map["marca"] = c;
            else if (header.Contains("procesador") && !map.ContainsKey("procesador")) map["procesador"] = c;
            else if (header.Contains("disco") && !map.ContainsKey("disco_duro")) map["disco_duro"] = c;
            else if ((header.Contains("ram") || header.Contains("memoria")) && !map.ContainsKey("memoria_ram")) map["memoria_ram"] = c;
            else if (header.Contains("descrip") && !map.ContainsKey("descripcion")) map["descripcion"] = c;
            else if (header.Contains("departamento") && !map.ContainsKey("departamento")) map["departamento"] = c;
            else if ((header.Contains("estatus") || header.Contains("estado")) && !map.ContainsKey("estatus_equipo")) map["estatus_equipo"] = c;
            else if (header.Contains("mantenimiento realizado") && !map.ContainsKey("mantenimiento_realizado")) map["mantenimiento_realizado"] = c;
            else if (header.Contains("tipo de mantenimiento") && !map.ContainsKey("tipo_mantenimiento")) map["tipo_mantenimiento"] = c;
            else if (header.Contains("fecha del mantenimiento") || header.Contains("fecha mantenimiento")) map["fecha_mantenimiento"] = c;
        }

        return map;
    }

    private static string? GetValue(DataRow row, Dictionary<string, int> map, string key)
    {
        if (map.TryGetValue(key, out var colIndex) && colIndex < row.ItemArray.Length)
        {
            var val = row[colIndex]?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(val) ? null : val;
        }
        return null;
    }

    private static bool? ParseBool(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var lower = val.Trim().ToLowerInvariant();
        return lower is "si" or "sí" or "true" or "1" or "yes";
    }

    private static DateOnly? ParseDate(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;

        // Si es número serial de Excel (ej. 46152)
        if (double.TryParse(val, out var serialDate) && serialDate > 20000 && serialDate < 100000)
        {
            try
            {
                var dt = DateTime.FromOADate(serialDate);
                return DateOnly.FromDateTime(dt);
            }
            catch { }
        }

        if (DateTime.TryParse(val, out var parsedDt))
        {
            return DateOnly.FromDateTime(parsedDt);
        }

        return null;
    }

    private string ResolveKeyPath(string path)
    {
        if (Path.IsPathRooted(path)) return path;
        return Path.Combine(_env.ContentRootPath, path);
    }
}
