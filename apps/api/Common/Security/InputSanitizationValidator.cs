using System.Text.RegularExpressions;

namespace PlataformaActas.Api.Common.Security;

public static class InputSanitizationValidator
{
    // Detecta patrones comunes de SQL Injection (tautologías, comentarios, palabras clave DDL/DML concatenadas)
    private static readonly Regex SqlInjectionRegex = new(
        @"(?i)(--|;|\/\*|\*\/|\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|TRUNCATE|UNION( +ALL)?|UPDATE)\b|'(\s*OR\s*|\s*AND\s*)|'1'='1'|1=1)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool ContainsSqlInjection(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return SqlInjectionRegex.IsMatch(input);
    }

    public static bool IsValidDocumentNumber(string? numeroDocumento)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            return true;
        }

        if (ContainsSqlInjection(numeroDocumento))
        {
            return false;
        }

        // Exclusivamente dígitos numéricos (0-9), longitud entre 6 y 12 dígitos
        return Regex.IsMatch(numeroDocumento.Trim(), @"^[0-9]{6,12}$");
    }

    // Regla 2: Nombres y Apellidos (2 a 100 caracteres alfabéticos, tildes, diéresis, espacios y apóstrofes)
    private static readonly Regex PersonNameRegex = new(
        @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s'-]{2,100}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValidPersonName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var trimmed = name.Trim();
        if (ContainsSqlInjection(trimmed)) return false;
        return PersonNameRegex.IsMatch(trimmed);
    }

    // Regla 3: Correos electrónicos según estándar RFC 5322
    private static readonly Regex Rfc5322EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var trimmed = email.Trim();
        if (trimmed.Length > 150 || ContainsSqlInjection(trimmed)) return false;
        return Rfc5322EmailRegex.IsMatch(trimmed);
    }

    // Regla 4: Números telefónicos / Móvil (10 dígitos estándar o prefijo internacional + sin espacios)
    private static readonly Regex PhoneRegex = new(
        @"^(\+?[0-9]{10,15})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return true;
        var trimmed = phone.Trim();
        if (ContainsSqlInjection(trimmed)) return false;
        return PhoneRegex.IsMatch(trimmed);
    }

    // Regla 5: Placas / Seriales / InventoryKey (alfanumérico limpio sin caracteres de inyección)
    private static readonly Regex InventoryKeyRegex = new(
        @"^[a-zA-Z0-9\-_./]{2,50}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValidInventoryKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        var trimmed = key.Trim();
        if (ContainsSqlInjection(trimmed)) return false;
        return InventoryKeyRegex.IsMatch(trimmed);
    }
}
