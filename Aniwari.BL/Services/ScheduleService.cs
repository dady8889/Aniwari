using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using System.Xml.Linq;
using JikanDotNet;
using Microsoft.Extensions.Logging;

namespace Aniwari.BL.Services;

public interface IScheduleService
{
    IAsyncEnumerable<KeyValuePair<ScheduleDay, AnimeSchedule>> GetSchedule(CancellationToken cancellationToken = default);
}

public class ScheduleService : IScheduleService
{
    private readonly IJikan _jikan;
    private readonly ILogger<ScheduleService> _logger;
    private readonly FixedWindowRateLimiter _limiter;
    private readonly FixedWindowRateLimiter _limiterMinute;

    public ScheduleService(ILogger<ScheduleService> logger, IJikan jikan)
    {
        _jikan = jikan;
        _logger = logger;
        _limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()
        {
            Window = TimeSpan.FromMilliseconds(750),
            AutoReplenishment = true,
            PermitLimit = 1,
            QueueLimit = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });

        _limiterMinute = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()
        {
            Window = TimeSpan.FromSeconds(60),
            AutoReplenishment = true,
            PermitLimit = 30,
            QueueLimit = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
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

    public async IAsyncEnumerable<KeyValuePair<ScheduleDay, AnimeSchedule>> GetSchedule([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool next = false;
        int page = 1;
        PaginatedJikanResponse<ICollection<Anime>> info;

        do
        {
            using RateLimitLease lease = await _limiter.AcquireAsync(1, cancellationToken);
            using RateLimitLease leaseMinute = await _limiterMinute.AcquireAsync(1, cancellationToken);

            // retry if the queue is full
            if (!lease.IsAcquired || !leaseMinute.IsAcquired)
            {
                next = true;
                await Task.Delay(100);
                continue;
            }
            try
            {
                info = await _jikan.GetScheduleAsync(page); // todo cts

            }
            catch (Exception ex)
            {
                _logger.LogError("Could not obtain schedule from Jikan. Exception: {}", ex.Message.ToString());
                throw;
            }

            next = info.Pagination.HasNextPage;

            foreach (Anime anime in info.Data)
            {
                AnimeSchedule animeSchedule = new();

                ScheduleDay day;
                if (!TryParseScheduleDay(anime.Broadcast.String, out day))
                {
                    _logger.LogDebug("Could not parse \"{}\" for anime \"{}\"", anime.Broadcast.String, anime.MalId);
                    continue;
                }

                animeSchedule.ScheduleDay = day;
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

                animeSchedule.RawAirTime = anime.Broadcast.String;

                yield return KeyValuePair.Create(day, animeSchedule);
            }

            page++;

            _logger.LogDebug("Free limits: S = {}, M = {}", _limiter.GetStatistics()?.CurrentAvailablePermits, _limiterMinute.GetStatistics()?.CurrentAvailablePermits);

            

        } while (next);
    }
}

public class AnimeSchedule
{
    public string Url { get; set; } = string.Empty;
    public ScheduleDay ScheduleDay { get; set; } = ScheduleDay.Monday;
    public ScheduleDay ConvertedScheduleDay { get; set; } = ScheduleDay.Monday;
    public Dictionary<TitleType, List<string>> Titles { get; set; } = new();
    public TimeOnly? AirTime { get; set; }
    public TimeOnly? ConvertedAirTime { get; set; }
    public string? Timezone { get; set; }
    public string RawAirTime { get; set; } = string.Empty;

    public string GetDefaultTitle()
    {
        return Titles[TitleType.Default][0];
    }
}

public enum ScheduleDay
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

public enum TitleType
{
    Default,
    English,
    Japanese,
    Synonym
}