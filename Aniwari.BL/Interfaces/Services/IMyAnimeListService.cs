using Aniwari.DAL.MyAnimeList;

namespace Aniwari.BL.Interfaces;

public interface IMyAnimeListService
{
    Task AddAnime(int malAnimeId);
    Task DeleteAnime(int malAnimeId);
    Task<bool> EditAnime(int malAnimeId, int watchedEpisodes, MALAnimeState status = MALAnimeState.Watching);
    Task<List<MALAnime>?> GetAnimeList(MALAnimeState state, int offset = 0);
    Task ImportList();
}
