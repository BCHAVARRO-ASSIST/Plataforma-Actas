using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using PlataformaActas.Api.Features.Actas;
using PlataformaActas.Api.Features.Auth;
using PlataformaActas.Api.Features.Dashboard;
using PlataformaActas.Api.Features.Equipment;
using PlataformaActas.Api.Features.Health;
using PlataformaActas.Api.Features.Reportes;
using PlataformaActas.Api.Features.Roles;
using PlataformaActas.Api.Features.Usuarios;
using PlataformaActas.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();


// =========================================================
// JWT
// =========================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No está configurada la clave JWT.");

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "PlataformaActas.Api";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "PlataformaActas.Web";

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();


// =========================================================
// CORS
// =========================================================

var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
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
// DATABASE
// =========================================================

var dataSource = DatabaseConnection.CreateDataSource(
    builder.Configuration);

builder.Services.AddSingleton(dataSource);


// =========================================================
// SERVICES & PDF CONFIGURATION
// =========================================================

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddScoped<ActaService>();
builder.Services.AddScoped<DocumentoActaService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReportesService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<EmailService>();


// =========================================================
// APPLICATION
// =========================================================

var app = builder.Build();


// =========================================================
// MIDDLEWARE
// =========================================================

app.UseExceptionHandler();

app.UseCors("WebAdmin");

app.UseAuthentication();

app.UseAuthorization();



// =========================================================
// OPENAPI
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// =========================================================
// ENDPOINTS
// =========================================================

app.MapHealthEndpoint();

app.MapDashboardEndpoints();

app.MapReportesEndpoints();

app.MapEquipmentEndpoints();

app.MapActaEndpoints();

app.MapUsuarioEndpoints();

app.MapRolEndpoints();

app.MapAuthEndpoints();


// =========================================================
// RUN
// =========================================================

await DatabaseSeeder.SeedAsync(dataSource);

app.Run();