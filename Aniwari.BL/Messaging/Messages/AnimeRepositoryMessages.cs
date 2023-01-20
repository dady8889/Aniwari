using Aniwari.DAL.Storage;

namespace Aniwari.BL.Messaging;

public record AnimeWatchingChanged(AniwariAnime Anime, bool Watching) : IMessage;
public record AnimeEpisodeChanged(AniwariAnime Anime, AniwariEpisode? OldEpisode, AniwariEpisode? NewEpisode) : IMessage;
