namespace ViaPadel.Core.Domain.Aggregates.Players;

public sealed class PlayerId : ValueObject
{
    private PlayerId() { }
    public Guid Value { get; }
    private PlayerId(Guid value) => Value = value;

    public static PlayerId New() => new(Guid.NewGuid());

    // Empty-Guid is a programming error, not a domain failure scenario -> plain guard, not a Result.
    public static PlayerId Of(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("PlayerId cannot be empty.", nameof(value));
        return new PlayerId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value.ToString();
    public static PlayerId From(Guid value) => new(value);
}
