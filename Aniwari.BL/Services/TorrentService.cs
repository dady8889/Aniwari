using Microsoft.Extensions.Logging;
using MonoTorrent;
using MonoTorrent.Client;

namespace Aniwari.BL.Services;

public interface ITorrentService
{
    Task DownloadMagnet(string magnet, string path, Action<double>? onUpdate = null, Action<string>? onFinished = null);
}

public class TorrentService : ITorrentService
{
    private readonly ILogger<TorrentService> _logger;
    private readonly ClientEngine _clientEngine;

    public TorrentService(ILogger<TorrentService> logger)
    {
        _logger = logger;

        EngineSettingsBuilder builder = new(new EngineSettings());
        builder.CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "cache\\");
        _clientEngine = new ClientEngine(builder.ToSettings());
    }

    public async Task DownloadMagnet(string magnet, string path, Action<double>? onUpdate = null, Action<string>? onFinished = null)
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
                    if (args.NewState == TorrentState.Seeding)
                    {
                        await torrent.StopAsync();
                        await _clientEngine.RemoveAsync(torrent);

                        onFinished?.Invoke(torrent.Files[0].Path);
                    }

                    onUpdate?.Invoke(torrent.Progress);
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not download the torrent. Exception: {}", ex.ToString());
            throw;
        }
    }
}
