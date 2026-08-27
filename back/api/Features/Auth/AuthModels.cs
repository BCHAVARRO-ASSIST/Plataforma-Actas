namespace PlataformaActas.Api.Features.Auth;

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


// =====================================================
// RECUPERACIÓN DE CONTRASEÑA
// =====================================================

public sealed record ForgotPasswordRequest(
    string Identifier
);

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword
);