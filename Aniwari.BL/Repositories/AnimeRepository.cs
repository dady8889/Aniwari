using Aniwari.BL.Interfaces;
using Aniwari.BL.Messaging;
using Aniwari.DAL.Jikan;
using Aniwari.DAL.Storage;
using Aniwari.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Aniwari.BL.Services;
using static System.Formats.Asn1.AsnWriter;

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

    public AniwariAnime AddAnime(JikanAnime jikanAnime)
    {
        var anime = _store.Animes.FirstOrDefault(x => x.Id == jikanAnime.MalId);

        var title = ((ITitle)jikanAnime).GetDefaultTitle();

        // if the anime is already in DB, check if it needs some updates
        if (anime != null)
        {
            if (anime.Title != title)
                anime.Title = title;

            if (anime.EpisodesCount != jikanAnime.Episodes)
                anime.EpisodesCount = jikanAnime.Episodes;

            if (anime.JSTAiredDate != jikanAnime.JSTAiredDate)
                anime.JSTAiredDate = jikanAnime.JSTAiredDate;

            if (anime.JSTScheduleDay != jikanAnime.JSTScheduleDay)
                anime.JSTScheduleDay = jikanAnime.JSTScheduleDay;

            if (anime.JSTAirTime != jikanAnime.JSTAirTime)
                anime.JSTAirTime = jikanAnime.JSTAirTime;

            if (anime.Timezone != jikanAnime.Timezone)
                anime.Timezone = jikanAnime.Timezone;

            (anime as ITimeConvertible).UpdateLocalTime();
            (anime as ITitle).UpdateTitles(jikanAnime.Titles);

            return anime;
        }

        var newAnime = new AniwariAnime(jikanAnime.MalId, title, jikanAnime.Episodes, $"{title} @ep", jikanAnime.JSTAiredDate, jikanAnime.JSTScheduleDay, jikanAnime.JSTAirTime, jikanAnime.Timezone);

        (newAnime as ITimeConvertible).UpdateLocalTime();
        (newAnime as ITitle).UpdateTitles(jikanAnime.Titles);

        _store.Animes.Add(newAnime);
        return newAnime;
    }

    public AniwariAnime? GetAnimeById(int id)
    {
        var anime = _store.Animes.FirstOrDefault(x => x.Id == id);
        return anime;
    }
}
