using Aniwari.DAL.Schedule;
using Aniwari.DAL.Storage;

namespace Aniwari.BL.Interfaces;

public interface IAnimeRepository
{
    void SetAnimeWatching(int id, bool watching);
    bool GetAnimeWatching(int id);
    Anime AddAnime(AnimeSchedule scheduledAnime);
    void AddEpisode(Anime anime, Episode episode);
    void RemoveEpisode(Anime anime, Episode episode);
}
