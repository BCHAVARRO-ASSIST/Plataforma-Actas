using System.Security.Cryptography;
using System.Text;

namespace PlataformaActas.Api.Infrastructure.Security;

public static class Sha256IntegrityService
{
    public static string ComputeHash(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string ComputeHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return ComputeHash(bytes);
    }

    public static bool VerifyHash(byte[] bytes, string expectedHash)
    {
        var computed = ComputeHash(bytes);
        return string.Equals(computed, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}
