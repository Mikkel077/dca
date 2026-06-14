namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

public sealed class ScheduleId : ValueObject
{
    private  ScheduleId() { }
    public Guid Value { get; }
    private ScheduleId(Guid value) => Value = value;

    public static ScheduleId New() => new(Guid.NewGuid());
    public static ScheduleId Of(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("ScheduleId cannot be empty.", nameof(value));
        return new ScheduleId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
    
    public static ScheduleId From(Guid value) => new(value);
}
