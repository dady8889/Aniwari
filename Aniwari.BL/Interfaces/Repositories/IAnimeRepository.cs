using Aniwari.DAL.Jikan;
using Aniwari.DAL.Storage;

namespace Aniwari.BL.Interfaces;

public interface IAnimeRepository
{
    void SetAnimeWatching(int id, bool watching);
    bool GetAnimeWatching(int id);
    AniwariAnime AddAnime(JikanAnime jikanAnime);
    void AddEpisode(AniwariAnime anime, AniwariEpisode episode);
    void RemoveEpisode(AniwariAnime anime, AniwariEpisode episode);
    AniwariAnime? GetAnimeById(int id);
}
