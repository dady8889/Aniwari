using Aniwari.BL.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.BL.Repositories;

public record AnimeWatchingChanged(SettingsStore.Anime Anime, bool Watching) : IMessage;

public interface IAnimeRepository
{
    void SetAnimeWatching(int id, bool watching);
    bool GetAnimeWatching(int id);
    void AddAnime(int id, string title, int? episodes);
}

public class AnimeRepository : IAnimeRepository
{
    private readonly ILogger<AnimeRepository> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IMessageBusService _messageBusService;
    private readonly SettingsStore _store;

    public AnimeRepository(ILogger<AnimeRepository> logger, ISettingsService settingsService, IMessageBusService messageBusService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _store = _settingsService.GetStore();
        _messageBusService = messageBusService;
    }

    public void SetAnimeWatching(int id, bool watching)
    {
        var x = _store.Animes.FirstOrDefault(x => x.Id == id);

        if (x == null)
        {
            _logger.LogError("Anime {} is not stored.", id);
            throw new Exception($"Anime {id} is not stored.");
        }

        x.Watching = watching;

        _messageBusService.Publish(new AnimeWatchingChanged(x, watching));
    }

    public bool GetAnimeWatching(int id)
    {
        return _store.Animes.FirstOrDefault(x => x.Id == id)?.Watching ?? false;
    }

    public void AddAnime(int id, string title, int? episodes)
    {
        var anime = _store.Animes.FirstOrDefault(x => x.Id == id);

        // if the anime is already in DB, check if it needs some updates
        if (anime != null)
        {
            if (anime.Title != title)
                anime.Title = title;

            if (anime.EpisodesCount != episodes)
                anime.EpisodesCount = episodes;

            return;
        }

        _store.Animes.Add(new SettingsStore.Anime(id, false, title, episodes));
    }
}
