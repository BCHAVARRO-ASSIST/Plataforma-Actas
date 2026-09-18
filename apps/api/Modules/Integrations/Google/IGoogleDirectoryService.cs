namespace PlataformaActas.Api.Modules.Integrations.Google;

public sealed record GoogleUserValidationResult(
    bool Exists,
    bool IsActive,
    string Email,
    string FullName,
    string GivenName,
    string FamilyName,
    string? ErrorMessage = null
);

public interface IGoogleDirectoryService
{
    Task<GoogleUserValidationResult> ValidateUserAsync(string email, CancellationToken cancellationToken = default);
}
