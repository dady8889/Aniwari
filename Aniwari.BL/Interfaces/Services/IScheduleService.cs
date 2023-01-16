using Aniwari.DAL.Schedule;

namespace Aniwari.BL.Interfaces;

public interface IScheduleService
{
    /// <summary>
    /// Loads the anime schedule into a list of <see cref="AnimeSchedule"/>. 
    /// </summary>
    IAsyncEnumerable<IList<AnimeSchedule>> GetSchedule(CancellationToken cancellationToken = default);
}