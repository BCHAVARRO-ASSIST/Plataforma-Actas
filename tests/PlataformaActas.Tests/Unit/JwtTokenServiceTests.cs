using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using PlataformaActas.Api.Modules.Auth.Models.Dto;
using PlataformaActas.Api.Modules.Auth.Services;
using Xunit;

namespace PlataformaActas.Tests.Unit;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ProducesValidJwt_WithOnlyNameIdentifierClaim()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "super_secret_jwt_key_that_is_at_least_32_characters_long!" },
            { "Jwt:Issuer", "PlataformaActas.Api" },
            { "Jwt:Audience", "PlataformaActas.Web" },
            { "Jwt:ExpiresMinutes", "30" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var tokenService = new JwtTokenService(configuration);

        var usuario = new UsuarioAuthResponse(
            Id: 101,
            Nombre: "Carlos",
            Apellido: "Perez",
            UsuarioRed: "cperez",
            Correo: "cperez@empresa.com",
            Proceso: "Tecnología"
        );

        var rol = new RolAuthResponse(
            Id: 2,
            Nombre: "Administrador",
            Descripcion: "Administrador de la plataforma"
        );

        var permisos = new List<string> { "actas.crear", "actas.firmar", "inventario.editar" };

        // Act
        var tokenString = tokenService.GenerateToken(usuario, rol, permisos);

        // Assert
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        // El token debe contener nameid
        var nameIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdClaim);
        Assert.Equal("101", nameIdClaim.Value);

        // El token debe contener name y role
        var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal("cperez", nameClaim.Value);

        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
        Assert.NotNull(roleClaim);
        Assert.Equal("Administrador", roleClaim.Value);

        // El token debe contener los claims permission
        var permissionClaims = jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        Assert.Contains("actas.crear", permissionClaims);
        Assert.Contains("actas.firmar", permissionClaims);
        Assert.Contains("inventario.editar", permissionClaims);
    }
}
