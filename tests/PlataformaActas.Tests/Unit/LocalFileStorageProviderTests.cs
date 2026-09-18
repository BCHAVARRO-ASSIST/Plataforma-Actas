using Microsoft.AspNetCore.Hosting;
using Moq;
using PlataformaActas.Api.Infrastructure.Storage;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public class LocalFileStorageProviderTests
{
    [Fact]
    public async Task GetActaDocumentAsync_WhenPathAttemptsTraversal_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var mockEnv = new Mock<IWebHostEnvironment>();
        var tempDir = Path.Combine(Path.GetTempPath(), "PlataformaActas_Test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        mockEnv.Setup(e => e.ContentRootPath).Returns(tempDir);
        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var provider = new LocalFileStorageProvider(mockEnv.Object, mockConfig.Object);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await provider.GetActaDocumentAsync("../../etc/passwd");
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await provider.GetActaDocumentAsync("../../../secret.json");
            });
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
