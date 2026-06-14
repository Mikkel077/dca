using ViaPadel.Core.QueryContracts;
using ViaPadel.Core.Tools.OperationResult;
using ViaPadel.Infrastructure.SqliteDmPersistence;

namespace ViaPadel.Infrastructure.EfcQueries;

public class OverviewOfDailyScheduleHandler(SqliteDmContext context)
    : IQueryHandler<OverviewOfDailySchedule.Query, OverviewOfDailySchedule.Answer>
{
    public Task<OverviewOfDailySchedule.Answer> HandleAsync<T>(OverviewOfDailySchedule.Query query)
    {
        var slots = new List<OverviewOfDailySchedule.TimeSlotInfo>
        {
            new(new(15, 0), new(15, 30), false),
            new(new(15, 30), new(16, 0), false),
            new(new(16, 0), new(16, 30), false),
            new(new(16, 30), new(17, 0), false),
            new(new(17, 0), new(17, 30), true),
            new(new(17, 30), new(18, 0), true),
            new(new(18, 0), new(18, 30), true),
            new(new(18, 30), new(19, 0), true),
        };

        var courts = new List<OverviewOfDailySchedule.CourtScheduleInfo>
        {
            new("S1", "Single 1", new List<OverviewOfDailySchedule.BookingInfo>
            {
                new(Guid.NewGuid(), new(15, 0), new(16, 0), "Troels"),
                new(Guid.NewGuid(), new(17, 0), new(18, 0), "Peter"),
            }),
            new("D4", "Double 4", new List<OverviewOfDailySchedule.BookingInfo>
            {
                new(Guid.NewGuid(), new(16, 0), new(18, 0), "Anna"),
            }),
        };

        var answer = new OverviewOfDailySchedule.Answer(
            new DateOnly(2025, 4, 9), slots, courts);

        return Task.FromResult(answer);
    }

    
    
}