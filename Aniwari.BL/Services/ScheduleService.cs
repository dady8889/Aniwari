using System.Globalization;
using System.Runtime.CompilerServices;
using Aniwari.BL.Interfaces;
using Aniwari.DAL.Schedule;
using JikanDotNet;
using Microsoft.Extensions.Logging;

namespace Aniwari.BL.Services;

public class ScheduleService : IScheduleService
{
    private readonly IJikan _jikan;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(ILogger<ScheduleService> logger, IJikan jikan)
    {
        _jikan = jikan;
        _logger = logger;
    }

    /// <summary>
    /// Parse day from raw MAL broadcast string.
    /// </summary>
    private bool TryParseScheduleDay(string raw, out ScheduleDay scheduleDay)
    {
        scheduleDay = ScheduleDay.Monday;

        if (string.IsNullOrEmpty(raw) || raw == "Unknown")
            return false;

        var rawItems = raw.Split(" ");

        if (rawItems.Length < 1)
            return false;

        // remove plural form from the day
        var day = rawItems[0][..^1];

        return Enum.TryParse(day, out scheduleDay);
    }

    public async IAsyncEnumerable<IList<KeyValuePair<ScheduleDay, AnimeSchedule>>> GetSchedule([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool next = true;
        int page = 1;
        PaginatedJikanResponse<ICollection<Anime>>? info = null;

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

            List<KeyValuePair<ScheduleDay, AnimeSchedule>> list = new();

            foreach (Anime anime in info.Data)
            {
                // ignore non-approved animes
                if (!anime.Approved)
                    continue;

                AnimeSchedule animeSchedule = new();

                ScheduleDay day;
                if (!TryParseScheduleDay(anime.Broadcast.String, out day))
                {
                    _logger.LogDebug("Could not parse \"{}\" for anime \"{}\"", anime.Broadcast.String, anime.MalId);
                    continue;
                }

                animeSchedule.MalId = Convert.ToInt32(anime.MalId);
                animeSchedule.Url = anime.Url;
                animeSchedule.ScheduleDay = day;
                animeSchedule.RawAirTime = anime.Broadcast.String;
                animeSchedule.Image = anime.Images.WebP.ImageUrl;
                animeSchedule.Synopsis = anime.Synopsis;
                animeSchedule.Episodes = anime.Episodes;
                animeSchedule.AiredDate = anime.Aired.From;

                animeSchedule.Titles = new Dictionary<TitleType, List<string>>();

                // initialize empty list for each title type
                foreach (TitleType type in Enum.GetValues(typeof(TitleType)))
                {
                    animeSchedule.Titles.Add(type, new List<string>());
                }

                foreach (var title in anime.Titles)
                {
                    if (!Enum.TryParse(title.Type, out TitleType language))
                        continue;

                    animeSchedule.Titles[language].Add(title.Title);
                }

                if (anime.Broadcast.Time != null && anime.Broadcast.Timezone != null)
                {
                    animeSchedule.AirTime = TimeOnly.Parse(anime.Broadcast.Time, CultureInfo.InvariantCulture);
                    animeSchedule.Timezone = anime.Broadcast.Timezone;

                    // convert JST to local time
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(anime.Broadcast.Timezone);

                    var sourceTime = DateTime.Today.Add(animeSchedule.AirTime.Value.ToTimeSpan());
                    sourceTime = DateTime.SpecifyKind(sourceTime, DateTimeKind.Unspecified);
                    var convertedTime = TimeZoneInfo.ConvertTime(sourceTime, tz, TimeZoneInfo.Local);

                    animeSchedule.ConvertedAirTime = TimeOnly.FromDateTime(convertedTime);

                    int currentDay = (int)animeSchedule.ScheduleDay;

                    // we have to check if the converted time shifted the day
                    if (convertedTime.Day > sourceTime.Day)
                        currentDay++;
                    else if (convertedTime.Day < sourceTime.Day)
                        currentDay--;

                    // fix overflow
                    if (currentDay < 0)
                        currentDay = 6;
                    else if (currentDay > 6)
                        currentDay = 0;

                    animeSchedule.ConvertedScheduleDay = (ScheduleDay)currentDay;
                }

                list.Add(KeyValuePair.Create(day, animeSchedule));
            }

            yield return list;

            page++;

        } while (next);
    }
}
