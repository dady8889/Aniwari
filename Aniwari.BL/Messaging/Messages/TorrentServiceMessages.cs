namespace Aniwari.BL.Messaging;

public record TorrentUpdated(int AnimeId, int EpisodeId, double Progress) : IMessage;
public record TorrentFinished(int AnimeId, int EpisodeId, string FilePath) : IMessage;
public record TorrentErrored(int AnimeId, int EpisodeId, string ErrorMessage) : IMessage;
