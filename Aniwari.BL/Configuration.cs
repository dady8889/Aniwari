using Aniwari.BL.Repositories;
using Aniwari.BL.Services;
using JikanDotNet;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Aniwari.BL;

public static class AniwariServicesConfiguration
{
    public static IServiceCollection AddAniwari(this IServiceCollection services)
    {
        services.AddSingleton<IJikan, Jikan>((IServiceProvider provider) =>
        {
            var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()
            {
                Window = TimeSpan.FromMilliseconds(750),
                AutoReplenishment = true,
                PermitLimit = 1,
                QueueLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });

            var client = new HttpClient(handler: new ClientSideRateLimitedHandler(limiter));
            client.BaseAddress = new Uri("https://api.jikan.moe/v4/");
            client.Timeout = TimeSpan.FromSeconds(10);

            var jikan = new Jikan(new JikanDotNet.Config.JikanClientConfiguration()
            {
                SuppressException = false
            }, client);

            return jikan;
        });

        services.AddSingleton<IScheduleService, ScheduleService>();
        services.AddSingleton<IStoreService, StoreService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IMessageBusService, MessageBusService>();
        services.AddSingleton<INyaaService, NyaaService>();
        services.AddSingleton<ITorrentService, TorrentService>();

        services.AddTransient<IAnimeRepository, AnimeRepository>();

        return services;
    }
}
