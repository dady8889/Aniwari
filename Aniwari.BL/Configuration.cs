using Aniwari.BL.Services;
using JikanDotNet;
using Microsoft.Extensions.DependencyInjection;

namespace Aniwari.BL;

public static class AniwariServicesConfiguration
{
    public static IServiceCollection AddAniwari(this IServiceCollection services)
    {
        services.AddSingleton<IJikan, Jikan>();

        services.AddSingleton<IScheduleService, ScheduleService>();

        return services;
    }
}
