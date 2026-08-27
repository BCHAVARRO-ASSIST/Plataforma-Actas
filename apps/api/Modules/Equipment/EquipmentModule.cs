using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlataformaActas.Api.Modules.Equipment.Data;
using PlataformaActas.Api.Modules.Equipment.Endpoints;
using PlataformaActas.Api.Modules.Equipment.Services;

namespace PlataformaActas.Api.Modules.Equipment;

public static class EquipmentModule
{
    public static IServiceCollection AddEquipmentModule(this IServiceCollection services)
    {
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        return services;
    }

    public static IEndpointRouteBuilder MapEquipmentModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapEquipmentEndpoints();
        return endpoints;
    }
}
