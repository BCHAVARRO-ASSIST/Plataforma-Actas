using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Infrastructure.Email;
using PlataformaActas.Api.Infrastructure.Persistence;

namespace PlataformaActas.Api.Infrastructure;

/// <summary>
/// Métodos de extensión para la configuración modular y desacoplada de la infraestructura de servicios del API.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Configura la infraestructura de seguridad: cabeceras proxy, rate limiting OWASP, autenticación JWT con cookies y CORS.
    /// </summary>
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
#pragma warning disable ASPDEPR005
            options.KnownNetworks.Clear();
#pragma warning restore ASPDEPR005
            options.KnownProxies.Clear();
        });

        // 1. Rate Limiting (Protección contra fuerza bruta y DoS)
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Política anti-fuerza bruta para login: máximo 5 intentos por minuto por cliente/IP
            options.AddPolicy("auth-limit", httpContext =>
            {
                var clientKey = httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? "anonymous-login-client";

                return RateLimitPartition.GetFixedWindowLimiter(
                    clientKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Política general global de API: 100 peticiones por minuto por cliente
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var path = httpContext.Request.Path.Value ?? string.Empty;
                if (path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetNoLimiter("no-global-limit-for-auth");
                }

                var clientKey = httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "authenticated-client"
                    : httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                        ?? "anonymous-general-client";

                return RateLimitPartition.GetFixedWindowLimiter(
                    clientKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        // 2. Autenticación JWT con soporte de cookies HttpOnly seguras
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("No está configurada la clave JWT.");

        if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        {
            throw new InvalidOperationException("La clave secreta JWT ('Jwt:Key') debe tener una longitud mínima de 256 bits (al menos 32 caracteres criptográficos).");
        }

        var jwtIssuer = configuration["Jwt:Issuer"] ?? "PlataformaActas.Api";
        var jwtAudience = configuration["Jwt:Audience"] ?? "PlataformaActas.Web";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token) &&
                            context.Request.Cookies.TryGetValue("actas_session_token", out var cookieToken) &&
                            !string.IsNullOrWhiteSpace(cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? context.Principal?.FindFirst("sub")?.Value;

                        if (!int.TryParse(userIdClaim, out var userId))
                        {
                            context.Fail("Identificador de usuario inválido en el token de sesión.");
                            return;
                        }

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<PlataformaActasDbContext>();
                        var isActivo = await dbContext.Usuarios
                            .AsNoTracking()
                            .Where(u => u.Id == userId)
                            .Select(u => (bool?)u.Activo)
                            .FirstOrDefaultAsync();

                        if (isActivo != true)
                        {
                            context.Fail("Usuario inactivo");
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var problem = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                            Title = "No autorizado",
                            Status = StatusCodes.Status401Unauthorized,
                            Detail = !string.IsNullOrWhiteSpace(context.ErrorDescription)
                                ? context.ErrorDescription
                                : "No se proporcionó una credencial de autenticación válida o la sesión ha expirado.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                        await Results.Json(problem, contentType: "application/problem+json", statusCode: StatusCodes.Status401Unauthorized).ExecuteAsync(context.HttpContext);
                    },
                    OnForbidden = async context =>
                    {
                        var problem = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                            Title = "Acceso prohibido",
                            Status = StatusCodes.Status403Forbidden,
                            Detail = "No cuenta con los privilegios o permisos necesarios para acceder a este recurso.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                        await Results.Json(problem, contentType: "application/problem+json", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(context.HttpContext);
                    }
                };
            });

        services.AddAuthorization();
        services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();

        // 3. Política de CORS con soporte de credenciales seguras
        services.AddCors(options =>
        {
            options.AddPolicy("WebAdmin", policy =>
            {
                var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                var defaultOrigins = new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost" };
                var allowedOrigins = configuredOrigins.Concat(defaultOrigins).Distinct().ToArray();

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowCredentials()
                    .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin")
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH");
            });
        });

        return services;
    }

    /// <summary>
    /// Configura la infraestructura de notificaciones por correo: cola en memoria, worker en background y adaptadores Gmail / SMTP.
    /// </summary>
    public static IServiceCollection AddEmailInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(options =>
        {
            configuration.GetSection(SmtpOptions.SectionName).Bind(options);
            configuration.GetSection(SmtpOptions.EmailSettingsSectionName).Bind(options);
        });
        services.Configure<EmailSettings>(options =>
        {
            configuration.GetSection(SmtpOptions.SectionName).Bind(options);
            configuration.GetSection(EmailSettings.SectionName).Bind(options);
        });

        services.AddSingleton<IEmailNotificationQueue, EmailNotificationQueue>();
        services.AddScoped<GmailEmailSenderService>();
        services.AddScoped<SmtpEmailSenderService>();
        services.AddScoped<IEmailSenderService, GmailEmailSenderService>();
        services.AddHostedService<EmailNotificationBackgroundService>();

        return services;
    }

    /// <summary>
    /// Aplica cabeceras de endurecimiento HTTP según estándares de seguridad OWASP Top 10.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

                headers.Remove("Server");
                headers.Remove("X-Powered-By");
                return Task.CompletedTask;
            });

            await next();
        });
    }

    /// <summary>
    /// Mapea la especificación OpenAPI y la interfaz de documentación interactiva Swagger UI.
    /// </summary>
    public static WebApplication MapSwaggerDocumentation(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapGet("/swagger", () => Results.Content("""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>Plataforma de Actas - Swagger UI (ASSIST)</title>
          <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui.css" />
          <style>
            body { margin: 0; padding: 0; background: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; }
            .topbar { background-color: #0f172a !important; padding: 12px 0; border-bottom: 2px solid #2563eb; }
            .topbar a span { font-weight: 800; font-size: 16px; color: #ffffff; letter-spacing: 0.5px; }
            .swagger-ui .info .title { color: #0f172a; font-weight: 800; }
            .swagger-ui .info { margin: 25px 0; }
            .swagger-ui .scheme-container { background: #ffffff; box-shadow: 0 1px 4px rgba(0,0,0,0.05); padding: 15px 0; }
            .swagger-ui .btn.authorize { color: #16a34a; border-color: #16a34a; }
            .swagger-ui .btn.authorize svg { fill: #16a34a; }
          </style>
        </head>
        <body>
          <div id="swagger-ui"></div>
          <script src="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui-bundle.js"></script>
          <script src="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui-standalone-preset.js"></script>
          <script>
            window.onload = () => {
              window.ui = SwaggerUIBundle({
                url: '/openapi/v1.json',
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                  SwaggerUIBundle.presets.apis,
                  SwaggerUIStandalonePreset
                ],
                layout: "StandaloneLayout",
                persistAuthorization: true,
                displayRequestDuration: true,
                filter: true
              });
            };
          </script>
        </body>
        </html>
        """, "text/html"));

        app.MapGet("/swagger/index.html", () => Results.Redirect("/swagger"));

        return app;
    }
}
