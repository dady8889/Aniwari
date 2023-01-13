using Aniwari.DAL.Schedule;

namespace Aniwari.BL.Interfaces;

public interface IScheduleService
{
    /// <summary>
    /// Loads the anime schedule into a list of key-value pairs, where key is the airing weekday and value is the <see cref="AnimeSchedule"/> object. 
    /// </summary>
    IAsyncEnumerable<IList<KeyValuePair<ScheduleDay, AnimeSchedule>>> GetSchedule(CancellationToken cancellationToken = default);
}