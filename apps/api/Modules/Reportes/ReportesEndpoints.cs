using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Authorization;
using PlataformaActas.Api.Modules.Reportes.Endpoints;
using PlataformaActas.Api.Modules.Reportes.Models.Dto;
using PlataformaActas.Api.Modules.Reportes.Services;

namespace PlataformaActas.Api.Modules.Reportes.Endpoints
{
    public static class ReportesEndpoints
    {
        public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/reportes");

            group.MapPost("/equipos", async (
                ReportFilterRequest filter,
                IReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetEquiposReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(Permissions.ReportesConsultar);

            group.MapPost("/actas", async (
                ReportFilterRequest filter,
                IReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetActasReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(Permissions.ReportesConsultar);

            group.MapPost("/entregas", async (
                ReportFilterRequest filter,
                IReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetEntregasReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(Permissions.ReportesConsultar);

            group.MapPost("/devoluciones", async (
                ReportFilterRequest filter,
                IReportesService service,
                CancellationToken cancellationToken) =>
            {
                var data = await service.GetDevolucionesReportAsync(filter, cancellationToken);
                return Results.Ok(data);
            })
            .RequirePermission(Permissions.ReportesConsultar);

            return endpoints;
        }
    }
}

namespace PlataformaActas.Api.Modules.Reportes
{
    public static class ReportesModule
    {
        public static IServiceCollection AddReportesModule(this IServiceCollection services)
        {
            services.AddScoped<IReportesService, ReportesService>();
            return services;
        }

        public static IEndpointRouteBuilder MapReportesModule(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapReportesEndpoints();
            return endpoints;
        }
    }
}
