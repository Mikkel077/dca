namespace ViaPadel.Core.Domain.Aggregates.Players;

/// <summary>UC9: runs until (and including) EndDate. No failure mode -> plain factory, not a Result.</summary>
public sealed class Quarantine : ValueObject
{
    private Quarantine() { }
    public DateOnly EndDate { get; }
    private Quarantine(DateOnly endDate) => EndDate = endDate;

    public static Quarantine StartingOn(DateOnly today) => new(today.AddDays(3)); // UC9 S1
    public Quarantine Extend(int days) => new(EndDate.AddDays(days));             // UC9 S2
    public bool IsActiveOn(DateOnly date) => date <= EndDate;

    protected override IEnumerable<object?> GetEqualityComponents() { yield return EndDate; }
}
