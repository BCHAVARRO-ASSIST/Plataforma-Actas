using System.Text;
using PlataformaActas.Api.Infrastructure.Security;
using Xunit;

namespace PlataformaActas.Api.UnitTests.Integrity;

public class Sha256IntegrityServiceTests
{
    [Fact]
    public void ComputeHash_WithValidString_ReturnsExpectedSha256Hex()
    {
        // Arrange
        var input = "PlataformaActas2026";

        // Act
        var hash = Sha256IntegrityService.ComputeHash(input);

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(64, hash.Length); // 64 hex chars for 256-bit hash
    }

    [Fact]
    public void ComputeHash_WithBytes_ReturnsDeterministicHash()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("Test Document Content");

        // Act
        var hash1 = Sha256IntegrityService.ComputeHash(bytes);
        var hash2 = Sha256IntegrityService.ComputeHash(bytes);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
