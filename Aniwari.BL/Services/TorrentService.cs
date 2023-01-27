using Aniwari.BL.Interfaces;
using Aniwari.BL.Messaging;
using Aniwari.DAL.Constants;
using Aniwari.DAL.Storage;
using Microsoft.Extensions.Logging;
using MonoTorrent;
using MonoTorrent.Client;

namespace Aniwari.BL.Services;

public class TorrentService : ITorrentService
{
    private readonly Timer _monitorTimer;
    private readonly ISettingsService _settingsService;
    private readonly IMessageBusService _messageBusService;
    private readonly ILogger<TorrentService> _logger;
    private readonly ClientEngine _clientEngine;
    private readonly Dictionary<ValueTuple<int, int>, TorrentManager> _torrentQueue = new();
    private readonly string _stateFilePath = Paths.StateFilePath;

    public TorrentService(ILogger<TorrentService> logger, IMessageBusService messageBusService, ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _messageBusService = messageBusService;
        _logger = logger;

        EngineSettingsBuilder builder = new(new EngineSettings());
        builder.CacheDirectory = Paths.TorrentCacheDirPath;
        _clientEngine = new ClientEngine(builder.ToSettings());

        _monitorTimer = new Timer(_ => MonitorTick(), null, 0, 30000);
    }

    #region Internal Methods

    private AniwariEpisode? InternalGetEpisode(int animeId, int episodeId)
    {
        var episode = _settingsService.GetStore().Animes.FirstOrDefault(x => x.Id == animeId)?.Episodes.FirstOrDefault(x => x.Id == episodeId);

        if (episode == null)
        {
            _logger.LogError("Episode {} for anime {} was not found.", episodeId, animeId);
        }

        return episode;
    }

    private int InternalGetBytesPerSecond(int kbps)
    {
        if (kbps < 0)
            return 0; // if -1 then unlimited 
        else if (kbps == 0)
            return 1; // if 0 then 1 bps, not possible to limit to 0 bytes
        else
            return kbps * 1024; // kbps to bps
    }

    private void InternalSetSettings(EngineSettingsBuilder builder)
    {
        var store = _settingsService.GetStore();
        if (store == null)
            throw new NullReferenceException();

        builder.MaximumUploadSpeed = InternalGetBytesPerSecond(store.MaximumUploadSpeed);
        builder.MaximumDownloadSpeed = InternalGetBytesPerSecond(store.MaximumDownloadSpeed);
    }

    private async Task<TorrentManager?> InternalGetTorrent(string magnet, string path, Action<PieceHashedEventArgs>? onUpdate = null, Action<TorrentStateChangedEventArgs>? onFinish = null, Action<string>? onError = null)
    {
        try
        {
            MagnetLink magnetLink = MagnetLink.Parse(magnet);
            var torrent = await _clientEngine.AddAsync(magnetLink, path).ConfigureAwait(false);
            if (torrent != null)
            {
                torrent.PieceHashed += (sender, args) =>
                {
                    onUpdate?.Invoke(args);
                };

                torrent.TorrentStateChanged += async (sender, args) =>
                {
                    _logger.LogDebug("Torrent changed state to {}", args.NewState.ToString());

                    if (args.NewState == TorrentState.Seeding)
                    {
                        onFinish?.Invoke(args);
                    }
                    else if (args.NewState == TorrentState.Error)
                    {
                        var exception = args.TorrentManager.Error.Exception;

                        _logger.LogDebug("Torrent errored. Exception: {}", exception.Message);

                        await torrent.StopAsync();
                        await _clientEngine.RemoveAsync(torrent);

                        onError?.Invoke(exception.Message);
                    }
                };

                return torrent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not download the torrent. Exception: {}", ex.ToString());
            throw;
        }

        return null;
    }

    #endregion

    public async Task DownloadAnime(int animeId, int episodeId, string magnet, string path)
    {
        _logger.LogDebug("Starting download for anime {} episode {}", animeId, episodeId);

        var key = (animeId, episodeId);
        var episode = InternalGetEpisode(animeId, episodeId);

        if (episode == null)
            return;

        var torrentHandle = await InternalGetTorrent(magnet, path,
           onUpdate: (e) =>
           {
               var progress = (int)e.TorrentManager.Progress;
               var prevProgress = episode.Progress;
               episode.Progress = progress;

               if (prevProgress != progress)
                   _messageBusService.Publish(new TorrentUpdated(animeId, episodeId, progress));
           },
           onFinish: async (e) =>
           {
               _logger.LogDebug("Download finished for anime {} episode {}", animeId, episodeId);

               if (e.TorrentManager == null || e.TorrentManager.Files == null || e.TorrentManager.Files.Count == 0)
               {
                   episode.Downloading = false;
                   episode.Downloaded = false;
                   episode.VideoFilePath = string.Empty;
                   await _settingsService.SaveAsync();

                   _torrentQueue.Remove(key);
                   _messageBusService.Publish(new TorrentErrored(animeId, episodeId, "Torrent has no files."));

                   return;
               }

               var filePath = e.TorrentManager.Files[0].Path;

               episode.Seeding = true;
               episode.Downloading = false;
               episode.Downloaded = true;
               episode.VideoFilePath = filePath;

               // update stats, check if ratio is bigger than max value, if yes then cancel
               await UpdateStats(e.TorrentManager, animeId, episodeId, false);

               await _settingsService.SaveAsync();

               _messageBusService.Publish(new TorrentFinished(animeId, episodeId, filePath));
           },
           onError: async (errorMessage) =>
           {
               _logger.LogDebug("Download errored for anime {} episode {}", animeId, episodeId);

               episode.Downloading = false;
               episode.Downloaded = false;
               episode.VideoFilePath = string.Empty;
               await _settingsService.SaveAsync();

               _torrentQueue.Remove(key);
               _messageBusService.Publish(new TorrentErrored(animeId, episodeId, errorMessage));
           }
        ).ConfigureAwait(false);

        if (torrentHandle == null)
        {
            _logger.LogError("Torrent for anime {} episode {} is invalid.", animeId, episodeId);
            throw new NullReferenceException($"Torrent for anime {animeId} episode {episodeId} is invalid.");
        }

        if (_torrentQueue.TryGetValue(key, out TorrentManager? torrent))
        {
            if (torrent.State != TorrentState.Stopped)
            {
                _logger.LogError("Torrent for anime {} episode {} is already active.", animeId, episodeId);
                throw new Exception($"Torrent for anime {animeId} episode {episodeId} is already active.");
            }

            _torrentQueue[key] = torrentHandle;
            return;
        }

        _torrentQueue.Add((animeId, episodeId), torrentHandle);

        await torrentHandle.StartAsync().ConfigureAwait(false);
    }

    public async Task<string?> CancelDownload(int animeId, int episodeId)
    {
        _logger.LogDebug("Cancelling download for anime {} episode {}", animeId, episodeId);

        var key = (animeId, episodeId);
        if (!_torrentQueue.TryGetValue(key, out TorrentManager? torrent))
        {
            _logger.LogError("Torrent for ({}, {}) has not been queued.", animeId, episodeId);
            throw new Exception($"Torrent for ({animeId}, {episodeId}) has not been queued.");
        }

        await torrent.StopAsync();
        await _clientEngine.RemoveAsync(torrent);

        _torrentQueue.Remove(key);

        _logger.LogDebug("Download canceled for anime {} episode {}", animeId, episodeId);

        if (torrent == null || torrent.Files == null || torrent.Files.Count == 0)
            return null;

        return torrent.Files[0].Path;
    }

    public async Task CancelSeed(int animeId, int episodeId)
    {
        _logger.LogDebug("Stopping seeding for anime {} episode {}", animeId, episodeId);

        var key = (animeId, episodeId);
        if (!_torrentQueue.TryGetValue(key, out TorrentManager? torrent))
        {
            _logger.LogError("Torrent for ({}, {}) has not been queued.", animeId, episodeId);
            throw new Exception($"Torrent for ({animeId}, {episodeId}) has not been queued.");
        }

        await torrent.StopAsync();
        await _clientEngine.RemoveAsync(torrent);

        _torrentQueue.Remove(key);

        var episode = InternalGetEpisode(animeId, episodeId);
        if (episode != null)
        {
            episode.Seeding = false;
        }

        _logger.LogDebug("Seeding stopped for anime {} episode {}", animeId, episodeId);
    }

    public async Task SaveStateAndExit()
    {
        await _clientEngine.StopAllAsync().ConfigureAwait(false);
        await _clientEngine.SaveStateAsync(_stateFilePath).ConfigureAwait(false);
    }

    public async Task RestoreState()
    {
        if (!File.Exists(_stateFilePath))
        {
            _logger.LogDebug("Could not find state file");
            return;
        }

        await ClientEngine.RestoreStateAsync(_stateFilePath).ConfigureAwait(false);

        var pausedEpisodes = _settingsService.GetStore().Animes.SelectMany(x => x.Episodes).Where(x => x.Downloading || x.Seeding).ToList();

        foreach (var ep in pausedEpisodes)
        {
            _logger.LogDebug("Restoring anime {} episode {}", ep.AnimeId, ep.Id);
            string archivePath = _settingsService.GetStore().ArchivePath;

            if (ep.TorrentMagnet == null)
                continue;

            await DownloadAnime(ep.AnimeId, ep.Id, ep.TorrentMagnet, archivePath).ConfigureAwait(false);
        }
    }

    public async Task ApplySettings()
    {
        _logger.LogDebug("Applying torrent settings from save");
        EngineSettingsBuilder builder = new(_clientEngine.Settings);
        InternalSetSettings(builder);
        await _clientEngine.UpdateSettingsAsync(builder.ToSettings());
    }

    #region Monitoring and statistics

    private async Task UpdateStats(TorrentManager torrent, int animeId, int episodeId, bool notify)
    {
        var episode = InternalGetEpisode(animeId, episodeId);

        if (episode == null)
            return;

        // update bytes received for seed ratio
        var mbytesReceived = (int)(torrent.Monitor.DataBytesDownloaded / (double)1024000);
        var mbytesSent = (int)(torrent.Monitor.DataBytesUploaded / (double)1024000);
        var receivedDiff = Math.Abs(episode.LastAddedBytesReceived - mbytesReceived);
        var sentDiff = Math.Abs(episode.LastAddedBytesSent - mbytesSent);

        episode.LastAddedBytesReceived += receivedDiff;
        episode.LastAddedBytesSent += sentDiff;
        episode.BytesReceived += receivedDiff;
        episode.BytesSent += sentDiff;

        var maxRatio = _settingsService.GetStore().MaximumSeedRatio;
        if (maxRatio >= 0 && episode.SeedRatio >= (double)maxRatio)
        {
            await CancelSeed(animeId, episodeId);
        }

        if (notify)
        {
            _messageBusService.Publish(new TorrentUpdated(animeId, episodeId, torrent.Progress));
        }
    }

    private async void MonitorTick()
    {
        foreach (var (key, torrent) in _torrentQueue)
        {
            if (torrent.State == TorrentState.Seeding)
            {
                // get more peers for seeding
                await torrent.DhtAnnounceAsync();
                await torrent.LocalPeerAnnounceAsync();

                await UpdateStats(torrent, key.Item1, key.Item2, true);
                await _settingsService.SaveAsync();
            }
        }
    }

    #endregion
}
