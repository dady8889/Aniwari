using Aniwari.DAL.Interfaces;

namespace Aniwari.DAL.Jikan;

public sealed class JikanAnime : ITimeConvertible, ITitle
{
    public JikanAnime()
    {
    }

    public int MalId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Synopsis { get; set; } = string.Empty;
    public Dictionary<TitleType, List<string>> Titles { get; set; } = new();
    public DayOfWeek JSTScheduleDay { get; set; } = DayOfWeek.Monday;
    public DayOfWeek LocalScheduleDay { get; set; } = DayOfWeek.Monday;
    public TimeOnly? JSTAirTime { get; set; }
    public TimeOnly? LocalAirTime { get; set; }
    public DateTime? JSTAiredDate { get; set; }
    public DateTime? LocalAiredDate { get; set; }
    public string? Timezone { get; set; }
    public string RawAirTime { get; set; } = string.Empty;
    public int? Episodes { get; set; }
}
