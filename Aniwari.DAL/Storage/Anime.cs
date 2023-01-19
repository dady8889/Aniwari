using Aniwari.DAL.Schedule;
using Aniwari.DAL.Interfaces;

namespace Aniwari.DAL.Storage;

public sealed class Anime : ITimeConvertible, ITitle
{
    public Anime()
    {
    }

    public Anime(int id, string title, int? episodesCount, string searchString, DateTime? jstAiredDate, DayOfWeek jstScheduleDay, TimeOnly? jstAirTime, string? timezone)
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

    public List<Episode> Episodes { get; set; } = new();

    public IEnumerable<Episode> GetWatchedEpisodes() => Episodes.Where(x => x.Watched);
}
