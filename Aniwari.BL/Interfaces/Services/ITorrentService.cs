namespace Aniwari.BL.Interfaces;

public interface ITorrentService
{
	/// <summary>
	/// Starts download of a specific anime episode.
	/// </summary>
	Task DownloadAnime(int animeId, int episodeId, string magnet, string path);

	/// <summary>
	/// Cancels download of a specific anime episode.
	/// </summary>
	Task<string?> CancelDownload(int animeId, int episodeId);

	/// <summary>
	/// Cancels seeding of a specific anime episode.
	/// </summary>
	Task CancelSeed(int animeId, int episodeId);

	/// <summary>
	/// Stops all active torrents and saves the state into disk.
	/// </summary>
	Task SaveStateAndExit();

    /// <summary>
    /// Restores torrents marked as Downloading or Seeding into the torrent queue.
    /// </summary>
    Task RestoreState();

	/// <summary>
	/// Applies current settings from <see cref="Aniwari.DAL.Storage.SettingsStore"/>, such as speed limits.
	/// </summary>
	Task ApplySettings();
}