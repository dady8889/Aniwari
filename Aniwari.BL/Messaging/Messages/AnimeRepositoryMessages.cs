using Aniwari.DAL.Storage;

namespace Aniwari.BL.Messaging;

public record AnimeWatchingChanged(Anime Anime, bool Watching) : IMessage;
public record AnimeEpisodeChanged(Anime Anime, Episode? OldEpisode, Episode? NewEpisode) : IMessage;
