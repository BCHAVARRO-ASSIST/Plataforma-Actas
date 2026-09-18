using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed class GoogleDirectoryService : IGoogleDirectoryService
{
    private readonly GoogleIntegrationOptions _options;
    private readonly IHostEnvironment _env;
    private readonly ILogger<GoogleDirectoryService> _logger;

    public GoogleDirectoryService(
        IOptions<GoogleIntegrationOptions> options,
        IHostEnvironment env,
        ILogger<GoogleDirectoryService> logger)
    {
        _options = options.Value;
        _env = env;
        _logger = logger;
    }

    public async Task<GoogleUserValidationResult> ValidateUserAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new GoogleUserValidationResult(false, false, "", "", "", "", "El correo no puede estar vacío.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!normalizedEmail.EndsWith($"@{_options.Domain}", StringComparison.OrdinalIgnoreCase))
        {
            return new GoogleUserValidationResult(false, false, normalizedEmail, "", "", "", $"El correo debe pertenecer al dominio corporativo @{_options.Domain}.");
        }

        var keyPath = ResolveKeyPath(_options.ServiceAccountKeyPath);
        if (!File.Exists(keyPath))
        {
            _logger.LogWarning("Archivo de credenciales de Google no encontrado en: {KeyPath}", keyPath);
            return new GoogleUserValidationResult(false, false, normalizedEmail, "", "", "", "Credenciales de Google no configuradas.");
        }

        try
        {
            var credential = GoogleCredential.FromFile(keyPath)
                .CreateScoped(DirectoryService.ScopeConstants.AdminDirectoryUserReadonly)
                .CreateWithUser(_options.AdminImpersonationEmail);

            using var directoryService = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PlataformaActas"
            });

            var request = directoryService.Users.Get(normalizedEmail);
            var user = await request.ExecuteAsync(cancellationToken);

            var isActive = user.Suspended != true;
            return new GoogleUserValidationResult(
                Exists: true,
                IsActive: isActive,
                Email: user.PrimaryEmail ?? normalizedEmail,
                FullName: user.Name?.FullName ?? normalizedEmail,
                GivenName: user.Name?.GivenName ?? "",
                FamilyName: user.Name?.FamilyName ?? ""
            );
        }
        catch (global::Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new GoogleUserValidationResult(false, false, normalizedEmail, "", "", "", "El colaborador no existe en Google Workspace.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar usuario en Google Directory: {Email}", normalizedEmail);
            return new GoogleUserValidationResult(false, false, normalizedEmail, "", "", "", $"Error al validar con Google: {ex.Message}");
        }
    }

    private string ResolveKeyPath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.Combine(_env.ContentRootPath, path);
    }
}
