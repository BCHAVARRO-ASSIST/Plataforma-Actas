using System.Security.Claims;
using PlataformaActas.Api.Authorization;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public class PermissionRequirementHandlerTests
{
    [Fact]
    public void CanAccessMensajeroResource_WhenUserHasAdminPermission_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new("permiso", Permissions.ActasConsultar)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = PermissionRequirementHandler.CanAccessMensajeroResource(principal, mensajeroUsuarioId: 99);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAccessMensajeroResource_WhenCourierAccessesOwnResource_ReturnsTrue()
    {
        // Arrange
        var courierId = 42;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, courierId.ToString()),
            new("permiso", Permissions.MensajeroActasConsultar)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = PermissionRequirementHandler.CanAccessMensajeroResource(principal, mensajeroUsuarioId: courierId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAccessMensajeroResource_WhenCourierAccessesOtherCourierResource_ReturnsFalse()
    {
        // Arrange
        var courierId = 42;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, courierId.ToString()),
            new("permiso", Permissions.MensajeroActasConsultar)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = PermissionRequirementHandler.CanAccessMensajeroResource(principal, mensajeroUsuarioId: 99);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanAccessMensajeroResource_WhenUserHasNoPermissions_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "10")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = PermissionRequirementHandler.CanAccessMensajeroResource(principal, mensajeroUsuarioId: 10);

        // Assert
        Assert.False(result);
    }
}
