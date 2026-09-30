using PlataformaActas.Api.Common.Security;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public class InputSanitizationValidatorTests
{
    [Theory]
    [InlineData("1018485291", true)]
    [InlineData("123456", true)]
    [InlineData("123456789012", true)]
    [InlineData("12345", false)] // Menor a 6 dígitos
    [InlineData("1234567890123", false)] // Mayor a 12 dígitos
    [InlineData("10184852A1", false)] // Contiene letras
    [InlineData("1018-485291", false)] // Contiene guiones
    [InlineData(" 1018485291 ", true)] // Se limpia con Trim()
    [InlineData("12345' OR '1'='1", false)] // SQL Injection
    [InlineData(null, true)] // Opcional
    [InlineData("", true)] // Opcional
    public void Test_IsValidDocumentNumber(string? doc, bool expected)
    {
        var result = InputSanitizationValidator.IsValidDocumentNumber(doc);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Juan", true)]
    [InlineData("María José", true)]
    [InlineData("Andrés-Trujillo", true)]
    [InlineData("O'Connor", true)]
    [InlineData("Agüero", true)]
    [InlineData("A", false)] // Longitud < 2
    [InlineData("Juan123", false)] // Contiene números
    [InlineData("Juan@Perez", false)] // Caracteres especiales prohibidos
    [InlineData("Juan; DROP TABLE usuarios; --", false)] // SQL Injection
    [InlineData(null, false)]
    [InlineData("", false)]
    public void Test_IsValidPersonName(string? name, bool expected)
    {
        var result = InputSanitizationValidator.IsValidPersonName(name);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("usuario@assist.com.co", true)]
    [InlineData("juan.perez+soporte@empresa.org", true)]
    [InlineData("invalido", false)]
    [InlineData("usuario@", false)]
    [InlineData("@dominio.com", false)]
    [InlineData("usuario@dominio", false)] // Falta TLD válido
    [InlineData("admin' OR '1'='1@assist.com.co", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void Test_IsValidEmail(string? email, bool expected)
    {
        var result = InputSanitizationValidator.IsValidEmail(email);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("3101234567", true)]
    [InlineData("+573101234567", true)]
    [InlineData("12345", false)] // < 10 dígitos
    [InlineData("310 123 4567", false)] // Espacios prohibidos
    [InlineData(null, true)] // Opcional
    [InlineData("", true)] // Opcional
    public void Test_IsValidPhoneNumber(string? phone, bool expected)
    {
        var result = InputSanitizationValidator.IsValidPhoneNumber(phone);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("LAP-2026-0042", true)]
    [InlineData("EQUIPO_01", true)]
    [InlineData("DEV.MAC.01", true)]
    [InlineData("A", false)] // Longitud < 2
    [InlineData("LAP'; DROP TABLE equipos; --", false)] // SQL Injection
    [InlineData(null, false)]
    [InlineData("", false)]
    public void Test_IsValidInventoryKey(string? key, bool expected)
    {
        var result = InputSanitizationValidator.IsValidInventoryKey(key);
        Assert.Equal(expected, result);
    }
}
