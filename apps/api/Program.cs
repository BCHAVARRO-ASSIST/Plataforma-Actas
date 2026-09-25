using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Actas;
using PlataformaActas.Api.Modules.Auth;
using PlataformaActas.Api.Modules.Dashboard;
using PlataformaActas.Api.Modules.Equipos;
using PlataformaActas.Api.Modules.Integrations;
using PlataformaActas.Api.Modules.Reportes;
using PlataformaActas.Api.Modules.Roles;
using PlataformaActas.Api.Modules.Usuarios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// =========================================================
// INFRAESTRUCTURA DE SEGURIDAD (OWASP, RATE LIMITING, JWT, CORS)
// =========================================================
builder.Services.AddSecurityInfrastructure(builder.Configuration);

// =========================================================
// BASE DE DATOS POSTGRESQL (EF CORE)
// =========================================================
var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("La cadena de conexión 'Postgres' no está configurada.");

builder.Services.AddDbContext<PlataformaActasDbContext>(options =>
    options.UseNpgsql(postgresConnection));

// =========================================================
// LICENCIAMIENTO QUESTPDF Y MÓDULOS DE NEGOCIO
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
// INFRAESTRUCTURA DE NOTIFICACIONES POR CORREO (GMAIL API / SMTP)
// =========================================================
builder.Services.AddEmailInfrastructure(builder.Configuration);

// =========================================================
// PIPELINE DE LA APLICACIÓN
// =========================================================
var app = builder.Build();

app.UseForwardedHeaders();
app.UseSecurityHeaders();
app.UseExceptionHandler();
app.UseCors("WebAdmin");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Documentación Swagger / OpenAPI
app.MapSwaggerDocumentation();

// =========================================================
// REGISTRO DE ENDPOINTS POR MÓDULOS
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