using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace PlataformaActas.Api.Features.Auth;

public sealed class PasswordResetService
{
    private readonly NpgsqlDataSource _dataSource;

    public PasswordResetService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<PasswordResetRequestResult>
        CreateResetRequestAsync(string identifier)
    {
        const string findUserSql = """
            SELECT
                "Id",
                "Correo",
                "Activo"
            FROM public.usuarios
            WHERE
                LOWER("Correo") = LOWER(@identifier)
                OR LOWER("UsuarioRed") = LOWER(@identifier)
            LIMIT 1;
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var findCommand =
            new NpgsqlCommand(
                findUserSql,
                connection);

        findCommand.Parameters.AddWithValue(
            "identifier",
            identifier.Trim());

        await using var reader =
            await findCommand.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return PasswordResetRequestResult.NotFound();
        }

        var usuarioId =
            reader.GetInt32(
                reader.GetOrdinal("Id"));

        var correo =
            reader.GetString(
                reader.GetOrdinal("Correo"));

        var activo =
            reader.GetBoolean(
                reader.GetOrdinal("Activo"));

        await reader.CloseAsync();

        if (!activo)
        {
            return PasswordResetRequestResult.NotFound();
        }

        // =====================================================
        // INVALIDAR SOLICITUDES ANTERIORES
        // =====================================================

        const string invalidateSql = """
            UPDATE public.password_reset_tokens
            SET "FechaUso" = NOW()
            WHERE
                "UsuarioId" = @usuarioId
                AND "FechaUso" IS NULL;
            """;

        await using var invalidateCommand =
            new NpgsqlCommand(
                invalidateSql,
                connection);

        invalidateCommand.Parameters.AddWithValue(
            "usuarioId",
            usuarioId);

        await invalidateCommand.ExecuteNonQueryAsync();

        // =====================================================
        // GENERAR TOKEN
        // =====================================================

        var tokenBytes =
            RandomNumberGenerator.GetBytes(32);

        var token =
            Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

        var tokenHash =
            ComputeHash(token);

        var expiration =
            DateTime.UtcNow.AddMinutes(30);

        // =====================================================
        // GUARDAR TOKEN
        // =====================================================

        const string insertSql = """
            INSERT INTO public.password_reset_tokens
            (
                "UsuarioId",
                "TokenHash",
                "FechaCreacion",
                "FechaExpiracion"
            )
            VALUES
            (
                @usuarioId,
                @tokenHash,
                NOW(),
                @fechaExpiracion
            );
            """;

        await using var insertCommand =
            new NpgsqlCommand(
                insertSql,
                connection);

        insertCommand.Parameters.AddWithValue(
            "usuarioId",
            usuarioId);

        insertCommand.Parameters.AddWithValue(
            "tokenHash",
            tokenHash);

        insertCommand.Parameters.AddWithValue(
            "fechaExpiracion",
            expiration);

        await insertCommand.ExecuteNonQueryAsync();

        return PasswordResetRequestResult.CreatedSuccessfully(
            correo,
            token,
            expiration);
    }


    // =========================================================
    // RESTABLECER CONTRASEÑA
    // =========================================================

    public async Task<PasswordResetResult>
        ResetPasswordAsync(
            string token,
            string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return PasswordResetResult.Invalid();
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return PasswordResetResult.Invalid();
        }

        var tokenHash =
            ComputeHash(token);

        const string findTokenSql = """
            SELECT
                prt."Id",
                prt."UsuarioId",
                prt."FechaExpiracion",
                prt."FechaUso"
            FROM public.password_reset_tokens prt
            WHERE
                prt."TokenHash" = @tokenHash
            LIMIT 1;
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var findCommand =
            new NpgsqlCommand(
                findTokenSql,
                connection);

        findCommand.Parameters.AddWithValue(
            "tokenHash",
            tokenHash);

        await using var reader =
            await findCommand.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return PasswordResetResult.Invalid();
        }

        var resetTokenId =
            reader.GetInt64(
                reader.GetOrdinal("Id"));

        var usuarioId =
            reader.GetInt32(
                reader.GetOrdinal("UsuarioId"));

        var expiration =
            reader.GetFieldValue<DateTime>(
                reader.GetOrdinal("FechaExpiracion"));

        var fechaUso =
            reader.IsDBNull(
                reader.GetOrdinal("FechaUso"))
                ? (DateTime?)null
                : reader.GetFieldValue<DateTime>(
                    reader.GetOrdinal("FechaUso"));

        await reader.CloseAsync();

        // =====================================================
        // VALIDAR TOKEN
        // =====================================================

        if (fechaUso.HasValue)
        {
            return PasswordResetResult.Invalid();
        }

        if (expiration <= DateTime.UtcNow)
        {
            return PasswordResetResult.ExpiredToken();
        }

        // =====================================================
        // GENERAR NUEVO HASH
        // =====================================================

        var passwordHash =
            BCrypt.Net.BCrypt.HashPassword(
                newPassword);

        await using var transaction =
            await connection.BeginTransactionAsync();

        // =====================================================
        // ACTUALIZAR CONTRASEÑA
        // =====================================================

        const string updatePasswordSql = """
            UPDATE public.usuarios
            SET
                "PasswordHash" = @passwordHash
            WHERE
                "Id" = @usuarioId
                AND "Activo" = TRUE;
            """;

        await using var updatePasswordCommand =
            new NpgsqlCommand(
                updatePasswordSql,
                connection,
                transaction);

        updatePasswordCommand.Parameters.AddWithValue(
            "passwordHash",
            passwordHash);

        updatePasswordCommand.Parameters.AddWithValue(
            "usuarioId",
            usuarioId);

        var updated =
            await updatePasswordCommand
                .ExecuteNonQueryAsync();

        if (updated != 1)
        {
            await transaction.RollbackAsync();

            return PasswordResetResult.Invalid();
        }

        // =====================================================
        // MARCAR TOKEN COMO UTILIZADO
        // =====================================================

        const string markUsedSql = """
            UPDATE public.password_reset_tokens
            SET
                "FechaUso" = NOW()
            WHERE
                "Id" = @tokenId;
            """;

        await using var markUsedCommand =
            new NpgsqlCommand(
                markUsedSql,
                connection,
                transaction);

        markUsedCommand.Parameters.AddWithValue(
            "tokenId",
            resetTokenId);

        await markUsedCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return PasswordResetResult.Completed();
    }


    // =========================================================
    // SHA-256 DEL TOKEN
    // =========================================================

    private static string ComputeHash(
        string value)
    {
        var bytes =
            Encoding.UTF8.GetBytes(value);

        var hash =
            SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}


// =============================================================
// RESULTADO DE SOLICITUD DE RECUPERACIÓN
// =============================================================

public sealed record PasswordResetRequestResult(
    bool Created,
    bool UserFound,
    string? Correo,
    string? Token,
    DateTime? Expiration)
{
    public static PasswordResetRequestResult NotFound() =>
        new(
            false,
            false,
            null,
            null,
            null);

    public static PasswordResetRequestResult
        CreatedSuccessfully(
            string correo,
            string token,
            DateTime expiration) =>
        new(
            true,
            true,
            correo,
            token,
            expiration);
}


// =============================================================
// RESULTADO DEL RESTABLECIMIENTO
// =============================================================

public sealed record PasswordResetResult(
    bool Successful,
    bool IsExpired)
{
    public static PasswordResetResult Completed() =>
        new(
            true,
            false);

    public static PasswordResetResult Invalid() =>
        new(
            false,
            false);

    public static PasswordResetResult ExpiredToken() =>
        new(
            false,
            true);
}