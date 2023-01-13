﻿using Aniwari.BL.Interfaces;
using Aniwari.BL.Messaging;
using Aniwari.BL.Services;
using Aniwari.DAL.Schedule;
using Aniwari.DAL.Storage;
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

    public void AddEpisode(Anime anime, Episode episode)
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

    public void RemoveEpisode(Anime anime, Episode episode)
    {
        if (!anime.Episodes.Any(ep => ep.Id == episode.Id))
            return;

        anime.Episodes.Remove(episode);

        _messageBusService.Publish(new AnimeEpisodeChanged(anime, episode, null));
    }

    public Anime AddAnime(AnimeSchedule scheduledAnime)
    {
        var anime = _store.Animes.FirstOrDefault(x => x.Id == scheduledAnime.MalId);

        var title = scheduledAnime.GetDefaultTitle();

        // if the anime is already in DB, check if it needs some updates
        if (anime != null)
        {
            if (anime.Title != title)
                anime.Title = title;

            if (anime.EpisodesCount != scheduledAnime.Episodes)
                anime.EpisodesCount = scheduledAnime.Episodes;

            if (anime.AiredDate != scheduledAnime.AiredDate)
                anime.AiredDate = scheduledAnime.AiredDate;

            if (anime.ScheduleDay != scheduledAnime.ScheduleDay.ToString())
                anime.ScheduleDay = scheduledAnime.ScheduleDay.ToString();

            if (anime.ScheduleTime != scheduledAnime.AirTime)
                anime.ScheduleTime = scheduledAnime.AirTime;

            return anime;
        }

        var newAnime = new Anime(scheduledAnime.MalId, title, scheduledAnime.Episodes, $"{title} @ep", scheduledAnime.AiredDate, scheduledAnime.ScheduleDay.ToString(), scheduledAnime.AirTime);

        _store.Animes.Add(newAnime);

        return newAnime;
    }
}
