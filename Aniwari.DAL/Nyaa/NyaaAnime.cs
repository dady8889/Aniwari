namespace Aniwari.DAL.Nyaa;

public sealed class NyaaAnime
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Magnet { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int Seeders { get; set; }
    public int Leechers { get; set; }
    public int CompletedDownloads { get; set; }
}
