using Aniwari.DAL.Jikan;
using Aniwari.DAL.Interfaces;

namespace Aniwari.DAL.Storage;

public sealed class AniwariAnime : ITimeConvertible, ITitle
{
    public AniwariAnime()
    {
    }

    public AniwariAnime(int id, string title, int? episodesCount, string searchString, DateTime? jstAiredDate, DayOfWeek jstScheduleDay, TimeOnly? jstAirTime, string? timezone)
    {
        Id = id;
        Title = title;
        EpisodesCount = episodesCount;
        SearchString = searchString;
        JSTAiredDate = jstAiredDate;
        JSTScheduleDay = jstScheduleDay;
        JSTAirTime = jstAirTime;
        Timezone = timezone;
    }

    public int Id { get; set; }
    public bool Watching { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SearchString { get; set; } = string.Empty;
    public int? EpisodesCount { get; set; }
    public int? CustomEpisodesCount { get; set; }

    #region ITimeConvertible

    public DateTime? JSTAiredDate { get; set; }
    public DateTime? LocalAiredDate { get; set; }
    public DayOfWeek JSTScheduleDay { get; set; }
    public DayOfWeek LocalScheduleDay { get; set; }
    public TimeOnly? JSTAirTime { get; set; }
    public TimeOnly? LocalAirTime { get; set; }
    public string? Timezone { get; set; }

    #endregion

    #region ITitle

    public Dictionary<TitleType, List<string>> Titles { get; set; } = new();

    #endregion

    public List<AniwariEpisode> Episodes { get; set; } = new();

    public IEnumerable<AniwariEpisode> GetWatchedEpisodes() => Episodes.Where(x => x.Watched);

    public int GetEstimatedAiredEpisodes()
    {
        var maxEpId = 0;

        if (LocalAiredDate == null)
        {
            return maxEpId;
        }

        // the aired date can differ with the scheduled time
        // we need to ignore this date and find the closest next day
        // we are assuming that the next episode airs at least after 7 next days
        DateTime dayAfterFirstAiring = LocalAiredDate.Value.Date.AddDays(7);

        while (dayAfterFirstAiring.DayOfWeek != LocalScheduleDay)
        {
            dayAfterFirstAiring = dayAfterFirstAiring.AddDays(1);
        }

        var betweenAiredToNow = DateTime.Today - dayAfterFirstAiring;
        var daysBetween = betweenAiredToNow.TotalDays;

        // we are still waiting for second episode
        if (daysBetween < 0)
        {
            maxEpId = 1;
        }
        else
        {
            maxEpId = (int)Math.Floor(daysBetween / 7) + 2;

            if (daysBetween % 7 == 0 && LocalAirTime != null && DateTime.Now.TimeOfDay < LocalAirTime.Value.ToTimeSpan())
            {
                maxEpId -= 1;
            }
        }

        // the anime has finished airing according to the current calendar
        if (EpisodesCount != null && maxEpId > EpisodesCount.Value)
        {
            maxEpId = EpisodesCount.Value;
        }

        return maxEpId;
    }
}
