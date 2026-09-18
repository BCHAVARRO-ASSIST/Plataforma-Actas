using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(
        UsuarioAuthResponse usuario,
        RolAuthResponse? rol,
        IReadOnlyList<string> permisos)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("No está configurada la clave JWT.");

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException("La clave secreta JWT ('Jwt:Key') debe tener una longitud mínima de 256 bits (al menos 32 caracteres criptográficos).");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "PlataformaActas.Api";
        var audience = _configuration["Jwt:Audience"] ?? "PlataformaActas.Web";

        var expiresMinutes =
            int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var minutes)
                ? minutes
                : 30;

        // El token contiene ÚNICAMENTE la referencia del identificador del usuario (nameid)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
