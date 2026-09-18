using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Actas;
using PlataformaActas.Api.Modules.Auth;
using PlataformaActas.Api.Modules.Dashboard;
using PlataformaActas.Api.Modules.Equipos;
using PlataformaActas.Api.Modules.Reportes;
using PlataformaActas.Api.Modules.Roles;
using PlataformaActas.Api.Modules.Usuarios;
using PlataformaActas.Api.Modules.Integrations;
using PlataformaActas.Api.Infrastructure.Email;

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// =========================================================
// RATE LIMITING (BRUTE FORCE PROTECTION)
// =========================================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth-limit", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});


// =========================================================
// JWT AUTHENTICATION
// =========================================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("No está configurada la clave JWT.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("La clave secreta JWT ('Jwt:Key') debe tener una longitud mínima de 256 bits (al menos 32 caracteres criptográficos).");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PlataformaActas.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PlataformaActas.Web";

builder.Services
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
                // Si el token no viene en el encabezado Authorization, extraerlo de la cookie HttpOnly
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
                    context.Fail("El usuario ha sido desactivado administrativamente o no existe.");
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, PlataformaActas.Api.Authorization.UserClaimsTransformation>();


// =========================================================
// CORS WITH CREDENTIALS
// =========================================================

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost", "http://192.168.2.75:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAdmin", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowCredentials()
            .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH");
    });
});


// =========================================================
// DATABASE PERSISTENCE (EF CORE)
// =========================================================

var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("La cadena de conexión 'Postgres' no está configurada.");

builder.Services.AddDbContext<PlataformaActasDbContext>(options =>
    options.UseNpgsql(postgresConnection));


// =========================================================
// QUESTPDF CONFIGURATION & MODULES REGISTRATION
// =========================================================

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddActaModule();
builder.Services.AddEquiposModule();
builder.Services.AddUsuarioModule();
builder.Services.AddRolModule();
builder.Services.AddAuthModule();
builder.Services.AddDashboardModule();
builder.Services.AddReportesModule();
builder.Services.AddIntegrationsModule(builder.Configuration);

// =========================================================
// EMAIL NOTIFICATION SERVICE & BACKGROUND QUEUE
// =========================================================

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<IEmailNotificationQueue, EmailNotificationQueue>();
builder.Services.AddScoped<IEmailSenderService, SmtpEmailSenderService>();
builder.Services.AddHostedService<EmailNotificationBackgroundService>();


// =========================================================
// APPLICATION PIPELINE
// =========================================================

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("WebAdmin");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

// =========================================================
// SWAGGER UI & OPENAPI SPECIFICATION
// =========================================================

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


// =========================================================
// ENDPOINTS
// =========================================================

app.MapHealthChecks("/api/health");
app.MapDashboardModule();
app.MapReportesModule();
app.MapEquiposModule();
app.MapActaModule();
app.MapUsuarioModule();
app.MapRolModule();
app.MapAuthModule();
app.MapIntegrationsModule();

// Sincronización automática de esquema y datos semilla en inicio
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PlataformaActasDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.Run();