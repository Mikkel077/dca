namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

public sealed class BookingId : ValueObject
{
    private BookingId() {}
    public Guid Value { get; }
    private BookingId(Guid value) => Value = value;

    public static BookingId New() => new(Guid.NewGuid());
    public static BookingId Of(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("BookingId cannot be empty.", nameof(value));
        return new BookingId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
    public static BookingId From(Guid value) => new(value);
}
