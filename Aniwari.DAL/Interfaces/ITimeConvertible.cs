using Aniwari.DAL.Schedule;

namespace Aniwari.DAL.Interfaces;

public interface ITimeConvertible
{
    public DayOfWeek JSTScheduleDay { get; set; }
    public DayOfWeek LocalScheduleDay { get; set; }
    public TimeOnly? JSTAirTime { get; set; }
    public TimeOnly? LocalAirTime { get; set; }
    public DateTime? JSTAiredDate { get; set; }
    public DateTime? LocalAiredDate { get; set; }
    public string? Timezone { get; set; }

    public void UpdateLocalTime()
    {
        if (Timezone == null)
            return;

        // convert JST to local time
        var tz = TimeZoneInfo.FindSystemTimeZoneById(Timezone);

        if (JSTAiredDate != null)
        {
            var sourceTime = DateTime.SpecifyKind(JSTAiredDate.Value, DateTimeKind.Unspecified);
            LocalAiredDate = TimeZoneInfo.ConvertTime(sourceTime, tz, TimeZoneInfo.Local);
        }

        if (JSTAirTime != null)
        {
            var sourceTime = DateTime.Today.Add(JSTAirTime.Value.ToTimeSpan());
            sourceTime = DateTime.SpecifyKind(sourceTime, DateTimeKind.Unspecified);
            var convertedTime = TimeZoneInfo.ConvertTime(sourceTime, tz, TimeZoneInfo.Local);

            LocalAirTime = TimeOnly.FromDateTime(convertedTime);

            int currentDay = (int)JSTScheduleDay;

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

            LocalScheduleDay = (DayOfWeek)currentDay;
        }
    }
}
