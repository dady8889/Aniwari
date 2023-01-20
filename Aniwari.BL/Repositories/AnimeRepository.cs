using Aniwari.BL.Interfaces;
using Aniwari.BL.Messaging;
using Aniwari.DAL.Jikan;
using Aniwari.DAL.Storage;
using Aniwari.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aniwari.BL.Repositories;

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

    public void AddEpisode(AniwariAnime anime, AniwariEpisode episode)
    {
        var ep = anime.Episodes.FirstOrDefault(ep => ep.Id == episode.Id);

        // if the episode is already in DB, check if it needs some updates
        if (ep != null)
        {
            bool changed = false;

            if (ep.TorrentMagnet != episode.TorrentMagnet)
            {
                ep.TorrentMagnet = episode.TorrentMagnet;
                changed = true;
            }

            if (ep.TorrentTitle != episode.TorrentTitle)
            {
                ep.TorrentTitle = episode.TorrentTitle;
                changed = true;
            }

            if (changed)
                _messageBusService.Publish(new AnimeEpisodeChanged(anime, null, ep));

            return;
        }

        anime.Episodes.Add(episode);

        _messageBusService.Publish(new AnimeEpisodeChanged(anime, null, episode));
    }

    public void RemoveEpisode(AniwariAnime anime, AniwariEpisode episode)
    {
        if (!anime.Episodes.Any(ep => ep.Id == episode.Id))
            return;

        anime.Episodes.Remove(episode);

        _messageBusService.Publish(new AnimeEpisodeChanged(anime, episode, null));
    }

    public AniwariAnime AddAnime(JikanAnime scheduledAnime)
    {
        var anime = _store.Animes.FirstOrDefault(x => x.Id == scheduledAnime.MalId);

        var title = ((ITitle)scheduledAnime).GetDefaultTitle();

        // if the anime is already in DB, check if it needs some updates
        if (anime != null)
        {
            if (anime.Title != title)
                anime.Title = title;

            if (anime.EpisodesCount != scheduledAnime.Episodes)
                anime.EpisodesCount = scheduledAnime.Episodes;

            if (anime.JSTAiredDate != scheduledAnime.JSTAiredDate)
                anime.JSTAiredDate = scheduledAnime.JSTAiredDate;

            if (anime.JSTScheduleDay != scheduledAnime.JSTScheduleDay)
                anime.JSTScheduleDay = scheduledAnime.JSTScheduleDay;

            if (anime.JSTAirTime != scheduledAnime.JSTAirTime)
                anime.JSTAirTime = scheduledAnime.JSTAirTime;

            if (anime.Timezone != scheduledAnime.Timezone)
                anime.Timezone = scheduledAnime.Timezone;

            (anime as ITimeConvertible).UpdateLocalTime();
            (anime as ITitle).UpdateTitles(scheduledAnime.Titles);

            return anime;
        }

        var newAnime = new AniwariAnime(scheduledAnime.MalId, title, scheduledAnime.Episodes, $"{title} @ep", scheduledAnime.JSTAiredDate, scheduledAnime.JSTScheduleDay, scheduledAnime.JSTAirTime, scheduledAnime.Timezone);

        (newAnime as ITimeConvertible).UpdateLocalTime();
        (newAnime as ITitle).UpdateTitles(scheduledAnime.Titles);

        _store.Animes.Add(newAnime);

        return newAnime;
    }
}
