using System.Globalization;
using System.Runtime.CompilerServices;
using Aniwari.BL.Interfaces;
using Aniwari.DAL.Jikan;
using Aniwari.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using IJikan = JikanDotNet.IJikan;

namespace Aniwari.BL.Services;

public class JikanService : IJikanService
{
    private readonly IJikan _jikan;
    private readonly ILogger<JikanService> _logger;

    public JikanService(ILogger<JikanService> logger, IJikan jikan)
    {
        _jikan = jikan;
        _logger = logger;
    }

    /// <summary>
    /// Parse day from raw MAL broadcast string.
    /// </summary>
    private bool TryParseScheduleDay(string raw, out DayOfWeek scheduleDay)
    {
        scheduleDay = DayOfWeek.Monday;

        if (string.IsNullOrEmpty(raw) || raw == "Unknown")
            return false;

        var rawItems = raw.Split(" ");

        if (rawItems.Length < 1)
            return false;

        // remove plural form from the day
        var day = rawItems[0][..^1];

        return Enum.TryParse(day, out scheduleDay);
    }

    private bool TryParseJikanAnime(JikanDotNet.Anime anime, out JikanAnime jikanAnime)
    {
        jikanAnime = new();

        if (anime.Broadcast?.String == null) // this anime is not scheduled
            return false;

        DayOfWeek day;
        if (!TryParseScheduleDay(anime.Broadcast.String, out day))
        {
            _logger.LogDebug("Could not parse \"{}\" for anime \"{}\"", anime.Broadcast.String, anime.MalId);
            return false;
        }

        jikanAnime.MalId = Convert.ToInt32(anime.MalId);
        jikanAnime.Url = anime.Url;
        jikanAnime.JSTScheduleDay = day;
        jikanAnime.RawAirTime = anime.Broadcast.String;
        jikanAnime.Image = anime.Images.WebP.ImageUrl;
        jikanAnime.Synopsis = anime.Synopsis;
        jikanAnime.Episodes = anime.Episodes;
        jikanAnime.JSTAiredDate = anime.Aired.From;

        jikanAnime.Titles = new Dictionary<TitleType, List<string>>();

        // initialize empty list for each title type
        foreach (TitleType type in Enum.GetValues(typeof(TitleType)))
        {
            jikanAnime.Titles.Add(type, new List<string>());
        }

        foreach (var title in anime.Titles)
        {
            if (!Enum.TryParse(title.Type, out TitleType language))
                continue;

            jikanAnime.Titles[language].Add(title.Title);
        }

        if (anime.Broadcast.Time != null && anime.Broadcast.Timezone != null)
        {
            jikanAnime.JSTAirTime = TimeOnly.Parse(anime.Broadcast.Time, CultureInfo.InvariantCulture);
            jikanAnime.Timezone = anime.Broadcast.Timezone;
            (jikanAnime as ITimeConvertible).UpdateLocalTime();
        }

        return true;
    }

    public async IAsyncEnumerable<IList<JikanAnime>> GetSchedule([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool next = true;
        int page = 1;
        JikanDotNet.PaginatedJikanResponse<ICollection<JikanDotNet.Anime>>? info = null;

        do
        {
            try
            {
                info = await _jikan.GetScheduleAsync(page, cancellationToken);
            }
            catch (HttpRequestException httpException)
            {
                if (httpException.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(500);
                    continue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not obtain schedule from Jikan. Exception: {}", ex.Message.ToString());
                yield break;
            }

            if (info == null)
                yield break;

            next = info.Pagination.HasNextPage;

            List<JikanAnime> list = new();

            foreach (JikanDotNet.Anime anime in info.Data)
            {
                // ignore non-approved animes
                if (!anime.Approved)
                    continue;

                if (!TryParseJikanAnime(anime, out JikanAnime jikanAnime))
                    continue;

                list.Add(jikanAnime);
            }

            yield return list;

            page++;

        } while (next);
    }

    public async Task<JikanAnime?> GetAnime(int malAnimeId, CancellationToken cancellationToken = default)
    {
        bool repeat = false;

        do
        {
            try
            {
                var data = await _jikan.GetAnimeAsync(malAnimeId, cancellationToken);

                // not found
                if (data == null)
                    return null;

                if(!TryParseJikanAnime(data.Data, out var jikanAnime))
                {
                    throw new Exception("Could not parse the Jikan response.");
                }

                return jikanAnime;
            }
            catch (HttpRequestException httpException)
            {
                if (httpException.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(500);
                    repeat = true;
                    continue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not obtain the data from Jikan. Exception: {}", ex.Message.ToString());
                return null;
            }
        } while (repeat);

        return null;
    }
}
