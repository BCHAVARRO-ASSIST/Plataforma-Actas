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

        // Permite números de documento válidos (letras, números, puntos y guiones, 3 a 30 caracteres)
        return Regex.IsMatch(numeroDocumento.Trim(), @"^[a-zA-Z0-9.\-_]{3,30}$");
    }
}
