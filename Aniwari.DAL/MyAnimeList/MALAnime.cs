using Aniwari.DAL.MyAnimeList;
using System.Text.Json.Serialization;

namespace Aniwari.DAL.MyAnimeList;

public sealed class MALAnime
{
    [JsonPropertyName("status")]
    public MALAnimeState Status { get; set; }

    [JsonPropertyName("num_watched_episodes")]
    public int WatchedEpisodes { get; set; }

    [JsonPropertyName("anime_id")]
    public int AnimeId { get; set; }
}
