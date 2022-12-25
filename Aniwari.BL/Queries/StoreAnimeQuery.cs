using Aniwari.BL.Services;

namespace Aniwari.BL.Queries;

public static class StoreAnimeQuery
{
    public static void SetAnimeWatching(this SettingsStore store, AnimeSchedule anime, bool watching)
    {
        var x = store.Animes.FirstOrDefault(x => x.Id == anime.MalId);

        if (x == null)
        {
            throw new Exception($"Anime {anime.MalId} is not stored.");
        }

        x.Watching = watching;
    }

    public static bool GetAnimeWatching(this SettingsStore store, AnimeSchedule anime)
    {
        return store.Animes.FirstOrDefault(x => x.Id == anime.MalId)?.Watching ?? false;
    }

    public static void AddAnime(this SettingsStore store, AnimeSchedule anime)
    {
        // if the anime is already in DB, ignore
        if (store.Animes.Any(x => x.Id == anime.MalId))
            return;

        store.Animes.Add(new SettingsStore.Anime(anime.MalId, false));
    }
}
