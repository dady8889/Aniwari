using Aniwari.DAL.Jikan;

namespace Aniwari.BL.Messaging;

public record JikanAnimeAdded(List<JikanAnime> Animes) : IMessage;

