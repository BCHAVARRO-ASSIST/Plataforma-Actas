using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Actas;
using PlataformaActas.Api.Modules.Auth;
using PlataformaActas.Api.Modules.Dashboard;
using PlataformaActas.Api.Modules.Equipment;
using PlataformaActas.Api.Modules.Reportes;
using PlataformaActas.Api.Modules.Roles;
using PlataformaActas.Api.Modules.Usuarios;

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
        opt.PermitLimit = 10;
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

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PlataformaActas.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PlataformaActas.Web";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, PlataformaActas.Api.Authorization.UserClaimsTransformation>();


// =========================================================
// CORS
// =========================================================

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAdmin", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
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
builder.Services.AddEquipmentModule();
builder.Services.AddUsuarioModule();
builder.Services.AddRolModule();
builder.Services.AddAuthModule();
builder.Services.AddDashboardModule();
builder.Services.AddReportesModule();


// =========================================================
// APPLICATION PIPELINE
// =========================================================

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("WebAdmin");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// =========================================================
// ENDPOINTS
// =========================================================

app.MapHealthChecks("/api/health");
app.MapDashboardModule();
app.MapReportesModule();
app.MapEquipmentModule();
app.MapActaModule();
app.MapUsuarioModule();
app.MapRolModule();
app.MapAuthModule();

// Sincronización automática de esquema y datos semilla en inicio
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PlataformaActasDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.Run();