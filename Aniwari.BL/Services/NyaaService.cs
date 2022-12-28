using Microsoft.Extensions.Logging;
using NyaaWrapper;

namespace Aniwari.BL.Services;

public interface INyaaService
{
    Task<List<NyaaAnime>> GetAnime(string title, int episode);
}

public class NyaaService : INyaaService
{
    private readonly ILogger<NyaaService> _logger;

    public NyaaService(ILogger<NyaaService> logger)
    {
        _logger = logger;
    }

    public async Task<List<NyaaAnime>> GetAnime(string title, int episode)
    {
        var result = new List<NyaaAnime>();

        try
        {
            string searchString = $"{title} {episode:D2}";

            Wrapper nyaaWrapper = new();

            var entries = await nyaaWrapper.GetNyaaEntries(new QueryOptions()
            {
                Category = NyaaWrapper.Enumerators.Categories.AnimeEnglishTranslated,
                Search = searchString
            });

            result = entries.Select(x => new NyaaAnime()
            {
                Id = x.Id,
                Category = x.Category,
                Url = x.Url,
                Name = x.Name,
                DownloadUrl = x.DownloadUrl,
                Magnet = x.Magnet,
                Size = x.Size,
                Date = x.Date,
                Seeders = x.Seeders,
                Leechers = x.Leechers,
                CompletedDownloads = x.CompletedDownloads,
            }).ToList();

            _logger.LogDebug("Found {} results for query {}", result.Count, searchString);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured while searching nyaa. Exception: {}", ex.ToString());
            throw;
        }

        return result;
    }
}

public class NyaaAnime
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
