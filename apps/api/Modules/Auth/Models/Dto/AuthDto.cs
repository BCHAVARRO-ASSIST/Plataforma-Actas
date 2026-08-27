namespace PlataformaActas.Api.Modules.Auth.Models.Dto;

public sealed record LoginRequest(
    string Usuario,
    string Password
);

public sealed record LoginResponse(
    string Token,
    UsuarioAuthResponse Usuario,
    RolAuthResponse Rol,
    IReadOnlyList<string> Permisos
);

public sealed record UsuarioAuthResponse(
    int Id,
    string Nombre,
    string Apellido,
    string UsuarioRed,
    string Correo,
    string Proceso
);

public sealed record RolAuthResponse(
    int Id,
    string Nombre,
    string Descripcion
);

public sealed record ForgotPasswordRequest(
    string Identifier
);

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword
);

public sealed record LoginData(
    UsuarioAuthResponse Usuario,
    RolAuthResponse Rol,
    IReadOnlyList<string> Permisos
);

public sealed record PasswordResetRequestResult(
    bool Created,
    bool UserFound,
    string? Correo,
    string? Token,
    DateTime? Expiration)
{
    public static PasswordResetRequestResult NotFound() =>
        new(false, false, null, null, null);

    public static PasswordResetRequestResult CreatedSuccessfully(
        string correo,
        string token,
        DateTime expiration) =>
        new(true, true, correo, token, expiration);
}

public sealed record PasswordResetResult(
    bool Successful,
    bool IsExpired)
{
    public static PasswordResetResult Completed() =>
        new(true, false);

    public static PasswordResetResult Invalid() =>
        new(false, false);

    public static PasswordResetResult ExpiredToken() =>
        new(false, true);
}
