using Aniwari.BL.Interfaces;
using Aniwari.DAL.Nyaa;
using Microsoft.Extensions.Logging;
using NyaaWrapper;

namespace Aniwari.BL.Services;

public class NyaaService : INyaaService
{
    private readonly ILogger<NyaaService> _logger;

    public NyaaService(ILogger<NyaaService> logger)
    {
        _logger = logger;
    }

    public async Task<List<NyaaAnime>> GetAnime(string title)
    {
        var result = new List<NyaaAnime>();

        try
        {
            Wrapper nyaaWrapper = new();

            var entries = await nyaaWrapper.GetNyaaEntries(new QueryOptions()
            {
                Category = NyaaWrapper.Enumerators.Categories.AnimeEnglishTranslated,
                Search = title
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

            _logger.LogDebug("Found {} results for query {}", result.Count, title);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured while searching nyaa. Exception: {}", ex.ToString());
            throw;
        }

        return result;
    }
}

