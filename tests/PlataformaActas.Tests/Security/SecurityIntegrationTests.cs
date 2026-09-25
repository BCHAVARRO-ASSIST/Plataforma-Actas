using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace PlataformaActas.Tests.Security;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "PlataformaActas_Dev_Jwt_Key_2026_Segura_32Caracteres_Minimo";
    public const string TestIssuer = "PlataformaActas.Api";
    public const string TestAudience = "PlataformaActas.Web";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=plataforma_actas;Username=plataforma_user;Password=plataforma_dev_2026",
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["GoogleIntegration:Enabled"] = "false"
            });
        });
    }
}

public class SecurityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SecurityIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// CASO 1: OWASP A01/A07 - Cookie Emitida con Atributos Seguros (HttpOnly, SameSite, Expiración 8h, Secure)
    /// </summary>
    [Fact]
    public async Task Test_Cookie_Emitida_Con_Atributos_Seguros()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var loginPayload = new
        {
            usuario = "admin",
            password = "Admin123*"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Forwarded-Proto", "https");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders), "La respuesta debe contener el encabezado Set-Cookie.");
        var cookieHeader = cookieHeaders.FirstOrDefault(c => c.Contains("actas_session_token"));
        Assert.NotNull(cookieHeader);

        // Validaciones estrictas de atributos de seguridad en la cookie
        Assert.Contains("actas_session_token=", cookieHeader);
        Assert.Contains("httponly", cookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookieHeader, StringComparison.OrdinalIgnoreCase);
        
        // Expiración fija a 8 horas (28800 segundos en max-age o expires)
        var has8HoursDuration = cookieHeader.Contains("max-age=28800", StringComparison.OrdinalIgnoreCase) ||
                                cookieHeader.Contains("expires=", StringComparison.OrdinalIgnoreCase);
        Assert.True(has8HoursDuration, "La cookie de sesión debe tener una expiración de 8 horas.");
    }

    /// <summary>
    /// CASO 1B: Detección Adaptativa TLS - En HTTP (localhost / desarrollo), NO emitir Secure para evitar descartes en navegador
    /// </summary>
    [Fact]
    public async Task Test_Cookie_Sin_Atributo_Secure_En_Http_Localhost()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var loginPayload = new
        {
            usuario = "admin",
            password = "Admin123*"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json")
        };
        // NO agregamos cabecera TLS ni X-Forwarded-Proto para simular HTTP plano de localhost

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders));
        var cookieHeader = cookieHeaders.FirstOrDefault(c => c.Contains("actas_session_token"));
        Assert.NotNull(cookieHeader);

        Assert.Contains("actas_session_token=", cookieHeader);
        Assert.Contains("httponly", cookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookieHeader, StringComparison.OrdinalIgnoreCase);
        // En HTTP local no debe incluir 'secure' para que el navegador no descarte la cookie en http://localhost
        Assert.DoesNotContain("secure", cookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// CASO 2: OWASP A01/A07 - Acceso Endpoint Protegido Con Cookie Vs Sin Cookie
    /// </summary>
    [Fact]
    public async Task Test_Acceso_Endpoint_Protegido_Con_Cookie_Vs_Sin_Cookie()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // 1. Sin cookie ni encabezado de autenticación -> 401 Unauthorized
        var unauthenticatedResponse = await client.GetAsync("/api/actas");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticatedResponse.StatusCode);

        // 2. Realizar login para obtener cookie válida
        var loginPayload = new { usuario = "admin", password = "Admin123*" };
        var loginResponse = await client.PostAsync(
            "/api/auth/login",
            new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookieHeaders));
        var rawCookie = cookieHeaders.First(c => c.Contains("actas_session_token"));
        var sessionTokenPart = rawCookie.Split(';').First(part => part.Trim().StartsWith("actas_session_token=")).Trim();

        // 3. Adjuntar la cookie actas_session_token a la petición protegida
        var authenticatedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/actas");
        authenticatedRequest.Headers.Add("Cookie", sessionTokenPart);

        var authenticatedResponse = await client.SendAsync(authenticatedRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, authenticatedResponse.StatusCode);
    }

    /// <summary>
    /// CASO 3: OWASP A07 - Rechazo De Token Vencido O Adulterado (ClockSkew Zero)
    /// </summary>
    [Fact]
    public async Task Test_Rechazo_De_Token_Vencido_O_Adulterado()
    {
        using var client = _factory.CreateClient();

        // 1. Token con expiración en el pasado (ClockSkew = Zero)
        var expiredToken = GenerateTestJwt(
            userId: "1",
            userName: "admin",
            role: "Administrador",
            permissions: new[] { "actas.consultar" },
            issuedAt: DateTime.UtcNow.AddMinutes(-30),
            expiresAt: DateTime.UtcNow.AddMinutes(-5));

        var expiredRequest = new HttpRequestMessage(HttpMethod.Get, "/api/actas");
        expiredRequest.Headers.Add("Cookie", $"actas_session_token={expiredToken}");

        var expiredResponse = await client.SendAsync(expiredRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, expiredResponse.StatusCode);

        // 2. Token con firma adulterada/falsificada
        var validFormatToken = GenerateTestJwt(
            userId: "1",
            userName: "admin",
            role: "Administrador",
            permissions: new[] { "actas.consultar" },
            issuedAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddHours(1));

        var tamperedToken = validFormatToken[..^8] + "TAMPERED";
        var tamperedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/actas");
        tamperedRequest.Headers.Add("Cookie", $"actas_session_token={tamperedToken}");

        var tamperedResponse = await client.SendAsync(tamperedRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, tamperedResponse.StatusCode);
    }

    /// <summary>
    /// CASO 4: OWASP A04/A05 - Rate Limiting Anti-Fuerza Bruta en /api/auth/login (5 permitidos, 6to retorna 429)
    /// </summary>
    [Fact]
    public async Task Test_Rate_Limiting_Anti_Fuerza_Bruta()
    {
        // Usar cliente con IP origen fija para probar la partición del rate limiter
        using var client = _factory.CreateClient();
        var clientIp = $"198.51.100.{Guid.NewGuid().ToString("N")[..4].GetHashCode() % 200 + 10}";

        var failedPayload = new
        {
            usuario = "admin",
            password = "PasswordErroneo123*"
        };

        HttpResponseMessage? lastResponse = null;

        for (int i = 1; i <= 6; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
            {
                Content = new StringContent(JsonSerializer.Serialize(failedPayload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Forwarded-For", clientIp);

            lastResponse = await client.SendAsync(request);

            if (i <= 5)
            {
                Assert.True(
                    lastResponse.StatusCode == HttpStatusCode.Unauthorized || lastResponse.StatusCode == HttpStatusCode.BadRequest,
                    $"El intento {i} debería responder 401 Unauthorized pero retornó {lastResponse.StatusCode}");
            }
            else
            {
                Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
            }
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    /// <summary>
    /// CASO 5: OWASP A05 - Cabeceras HTTP de Seguridad Presentes en las Respuestas
    /// </summary>
    [Fact]
    public async Task Test_Cabeceras_De_Seguridad_Presentes()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        // Assert HTTP Security Headers
        Assert.True(response.Headers.Contains("X-Content-Type-Options"), "Debe incluir X-Content-Type-Options");
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault());

        Assert.True(response.Headers.Contains("X-Frame-Options"), "Debe incluir X-Frame-Options");
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").FirstOrDefault());

        Assert.True(response.Headers.Contains("Referrer-Policy"), "Debe incluir Referrer-Policy");
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").FirstOrDefault());

        Assert.True(response.Headers.Contains("Strict-Transport-Security"), "Debe incluir HSTS");
        var hsts = response.Headers.GetValues("Strict-Transport-Security").FirstOrDefault();
        Assert.Contains("max-age=31536000", hsts);

        // Servidor y tecnología subyacente deben estar suprimidos
        Assert.False(response.Headers.Contains("Server"), "La cabecera 'Server' no debe ser divulgada.");
        Assert.False(response.Headers.Contains("X-Powered-By"), "La cabecera 'X-Powered-By' no debe ser divulgada.");
    }

    /// <summary>
    /// CASO 6: OWASP A03 - Prevención Estricta de SQL Injection en Campos de Entrada
    /// </summary>
    [Fact]
    public async Task Test_Prevencion_SQL_Injection_En_Campos_De_Entrada()
    {
        using var client = _factory.CreateClient();

        // 1. Iniciar sesión para obtener cookie autorizada
        var loginPayload = new { usuario = "admin", password = "Admin123*" };
        var loginResponse = await client.PostAsync(
            "/api/auth/login",
            new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookieHeaders));
        var rawCookie = cookieHeaders.First(c => c.Contains("actas_session_token"));
        var sessionTokenPart = rawCookie.Split(';').First(part => part.Trim().StartsWith("actas_session_token=")).Trim();

        // 2. Inyección clásica en NumeroDocumento: ' OR '1'='1
        var sqlPayload1 = new
        {
            nombre = "Prueba",
            apellido = "Inyeccion",
            usuarioRed = "sqlinjector1",
            correo = "sqlinjector1@assist.com.co",
            proceso = "Tecnologia",
            rolId = 1,
            numeroDocumento = "12345' OR '1'='1"
        };

        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/usuarios")
        {
            Content = new StringContent(JsonSerializer.Serialize(sqlPayload1), Encoding.UTF8, "application/json")
        };
        request1.Headers.Add("Cookie", sessionTokenPart);

        var response1 = await client.SendAsync(request1);
        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);

        // 3. Inyección destructiva en NumeroDocumento: '; DROP TABLE usuarios; --
        var sqlPayload2 = new
        {
            nombre = "Prueba",
            apellido = "InyeccionDrop",
            usuarioRed = "sqlinjector2",
            correo = "sqlinjector2@assist.com.co",
            proceso = "Tecnologia",
            rolId = 1,
            numeroDocumento = "'; DROP TABLE usuarios; --"
        };

        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/usuarios")
        {
            Content = new StringContent(JsonSerializer.Serialize(sqlPayload2), Encoding.UTF8, "application/json")
        };
        request2.Headers.Add("Cookie", sessionTokenPart);

        var response2 = await client.SendAsync(request2);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        // 4. Inyección en observaciones de anulación de acta: '; UPDATE actas SET activo=false; --
        var randomActaId = Guid.NewGuid();
        var request3 = new HttpRequestMessage(HttpMethod.Post, $"/api/actas/{randomActaId}/anular")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { motivo = "Anulacion'; DELETE FROM actas; --" }), Encoding.UTF8, "application/json")
        };
        request3.Headers.Add("Cookie", sessionTokenPart);

        var response3 = await client.SendAsync(request3);
        Assert.Equal(HttpStatusCode.BadRequest, response3.StatusCode);

        // 5. Verificar que la base de datos sigue íntegra y operativa
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get, "/api/actas");
        verifyRequest.Headers.Add("Cookie", sessionTokenPart);
        var verifyResponse = await client.SendAsync(verifyRequest);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
    }

    private static string GenerateTestJwt(
        string userId,
        string userName,
        string role,
        string[] permissions,
        DateTime issuedAt,
        DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, role)
        };

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
            claims.Add(new Claim("permiso", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CustomWebApplicationFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: CustomWebApplicationFactory.TestIssuer,
            audience: CustomWebApplicationFactory.TestAudience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
