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
    void AddAnime(int id, string title);
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

    public void AddAnime(int id, string title)
    {
        // if the anime is already in DB, ignore
        if (_store.Animes.Any(x => x.Id == id))
            return;

        _store.Animes.Add(new SettingsStore.Anime(id, false, title));
    }
}
