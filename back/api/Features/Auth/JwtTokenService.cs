using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PlataformaActas.Api.Features.Auth;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(
        UsuarioAuthResponse usuario,
        RolAuthResponse rol,
        IReadOnlyList<string> permisos)
    {
        var secret =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No está configurada la clave JWT.");

        var issuer =
            _configuration["Jwt:Issuer"]
            ?? "PlataformaActas.Api";

        var audience =
            _configuration["Jwt:Audience"]
            ?? "PlataformaActas.Web";

        var expiresMinutes =
            _configuration.GetValue<int?>(
                "Jwt:ExpiresMinutes")
            ?? 60;

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()),

            new(
                ClaimTypes.Name,
                usuario.UsuarioRed),

            new(
                ClaimTypes.Email,
                usuario.Correo),

            new(
                "nombre",
                usuario.Nombre),

            new(
                "apellido",
                usuario.Apellido),

            new(
                ClaimTypes.Role,
                rol.Nombre),

            new(
                "rol_id",
                rol.Id.ToString())
        };

        foreach (var permiso in permisos)
        {
            claims.Add(
                new Claim(
                    "permiso",
                    permiso));
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    expiresMinutes),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}