using ViaPadel.Core.Domain;
using ViaPadel.Core.Domain.Aggregates.DailySchedules;

namespace ViaPadel.Core.Domain.Contracts;

public interface IDailyScheduleCommands
{
    Task<DailySchedule?> GetByIdAsync(Guid id);

    Task<DailySchedule?> GetByDateAsync(DateOnly date);

    Task<IReadOnlyList<DailySchedule>> GetActiveByDateAsync(DateOnly date);

    Task<IReadOnlyList<DailySchedule>> GetSchedulesWithBookingByPlayerAsync(string playerEmail);

    Task AddAsync(DailySchedule schedule);
    Task UpdateAsync(DailySchedule schedule);
    
    Task SaveChangesAsync();
}
