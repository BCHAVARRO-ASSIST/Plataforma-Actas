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
        RolAuthResponse rol,
        IReadOnlyList<string> permisos)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("No está configurada la clave JWT.");

        var issuer = _configuration["Jwt:Issuer"] ?? "PlataformaActas.Api";
        var audience = _configuration["Jwt:Audience"] ?? "PlataformaActas.Web";

        var expiresMinutes =
            int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var minutes)
                ? minutes
                : 60;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.UsuarioRed),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, rol.Nombre),
            new("proceso", usuario.Proceso),
            new("rol_id", rol.Id.ToString())
        };

        foreach (var permiso in permisos)
        {
            claims.Add(new Claim("permiso", permiso));
        }

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
