namespace Aniwari.BL.Interfaces;

public interface ITorrentService
{
	Task DownloadAnime(int animeId, int episodeId, string magnet, string path);
	Task<string?> CancelDownload(int animeId, int episodeId);
	Task CancelSeed(int animeId, int episodeId);
	Task SaveAndExit();
	Task Restore();
}