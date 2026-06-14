namespace ViaPadel.Core.Domain.Aggregates.DailySchedules.Contracts;

public interface IDailyScheduleRepository
{
    Task<DailySchedule> Get(Guid id);
    Task Remove(Guid id);
    Task Add(DailySchedule schedule);
}