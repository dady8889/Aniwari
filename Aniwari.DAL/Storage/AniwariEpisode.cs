using System.Text.Json.Serialization;

namespace Aniwari.DAL.Storage;

public sealed class AniwariEpisode
{
    [JsonIgnore] public int LastAddedBytesSent { get; set; }
    [JsonIgnore] public int LastAddedBytesReceived { get; set; }
    [JsonIgnore] public int Progress { get; set; }
    public int Id { get; set; }
    public int AnimeId { get; set; }
    public bool Watched { get; set; }
    public bool Downloaded { get; set; }
    public bool Downloading { get; set; }
    public bool Seeding { get; set; }
    public int BytesSent { get; set; }
    public int BytesReceived { get; set; }
    public string? TorrentTitle { get; set; } = null;
    public string? TorrentMagnet { get; set; } = null;
    public string? VideoFilePath { get; set; } = null;

    [JsonIgnore] public double SeedRatio => BytesReceived <= 0 ? 0 : BytesSent / (double)BytesReceived;
}
