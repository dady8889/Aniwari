using Aniwari.DAL.Jikan;

namespace Aniwari.BL.Interfaces;

public interface IJikanService
{
    /// <summary>
    /// Loads the anime schedule into a list of <see cref="JikanAnime"/>. 
    /// </summary>
    IAsyncEnumerable<List<JikanAnime>> GetSchedule(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about anime from Jikan.
    /// </summary>
    Task<JikanAnime?> GetAnime(int malAnimeId, CancellationToken cancellationToken = default);
}