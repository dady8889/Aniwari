using Microsoft.Extensions.Logging;
using MonoTorrent;
using MonoTorrent.Client;

namespace Aniwari.BL.Services;

public interface ITorrentService
{
    Task DownloadAnime(int animeId, int episodeId, string magnet, string path, Action<double>? onUpdate = null, Action<string>? onFinish = null, Action<string>? onError = null);
    Task<string> CancelDownload(int animeId, int episodeId);
    Task SaveAndExit();
    Task Restore();
}

public class TorrentService : ITorrentService
{
    private readonly ILogger<TorrentService> _logger;
    private readonly ClientEngine _clientEngine;
    private readonly Dictionary<ValueTuple<int, int>, TorrentManager> _torrentQueue = new();
    private readonly string _stateFilePath;

    public TorrentService(ILogger<TorrentService> logger)
    {
        _logger = logger;

        _stateFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "state.dat");

        EngineSettingsBuilder builder = new(new EngineSettings());
        builder.CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "cache\\");
        _clientEngine = new ClientEngine(builder.ToSettings());
    }

    private async Task<TorrentManager?> DownloadMagnet(string magnet, string path, Action<double>? onUpdate = null, Action<string>? onFinish = null, Action<string>? onError = null)
    {
        try
        {
            MagnetLink magnetLink = MagnetLink.Parse(magnet);
            var torrent = await _clientEngine.AddAsync(magnetLink, path);
            if (torrent != null)
            {
                await torrent.StartAsync();

                torrent.PieceHashed += (sender, args) =>
                {
                    onUpdate?.Invoke(torrent.Progress);
                };

                torrent.TorrentStateChanged += async (sender, args) =>
                {
                    _logger.LogDebug("Torrent changed state to {}", args.NewState.ToString());

                    if (args.NewState == TorrentState.Seeding)
                    {
                        await torrent.StopAsync();
                        await _clientEngine.RemoveAsync(torrent);

                        onFinish?.Invoke(torrent.Files[0].Path);
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

    public async Task DownloadAnime(int animeId, int episodeId, string magnet, string path, Action<double>? onUpdate = null, Action<string>? onFinish = null, Action<string>? onError = null)
    {
        var key = (animeId, episodeId);
        var torrentHandle = await DownloadMagnet(magnet, path, onUpdate, onFinish, onError);

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
    }

    public async Task<string> CancelDownload(int animeId, int episodeId)
    {
        var key = (animeId, episodeId);
        if (!_torrentQueue.TryGetValue(key, out TorrentManager? torrent))
        {
            _logger.LogError("Torrent for ({}, {}) has not been queued.", animeId, episodeId);
            throw new Exception($"Torrent for ({animeId}, {episodeId}) has not been queued.");
        }

        await torrent.StopAsync();
        await _clientEngine.RemoveAsync(torrent);

        _torrentQueue.Remove(key);

        return torrent.Files[0].Path;
    }

    public async Task SaveAndExit()
    {
        await _clientEngine.StopAllAsync().ConfigureAwait(false);
        await _clientEngine.SaveStateAsync(_stateFilePath).ConfigureAwait(false);
    }

    public async Task Restore()
    {
        if (!File.Exists(_stateFilePath))
            return;

        await ClientEngine.RestoreStateAsync(_stateFilePath).ConfigureAwait(false);
    }
}
