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
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            context.ProblemDetails.Title = "Error interno del servidor";
            context.ProblemDetails.Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1";
            if (!builder.Environment.IsDevelopment())
            {
                context.ProblemDetails.Detail = "Ha ocurrido un error inesperado al procesar la solicitud. Por favor contacte al administrador con el identificador de rastreo.";
            }
        }
    };
});
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
try
{
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
}
catch (Exception)
{
    // Resiliencia ante entornos con políticas de AppLocker restrictivas en pruebas o sin binarios nativos
}

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
app.UseStatusCodePages();
app.UseCors("WebAdmin");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Documentación Swagger / OpenAPI (Habilitada en Desarrollo o explícitamente por configuración)
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", false))
{
    app.MapSwaggerDocumentation();
}

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