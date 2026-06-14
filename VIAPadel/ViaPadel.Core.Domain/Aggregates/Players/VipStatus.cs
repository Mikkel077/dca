namespace ViaPadel.Core.Domain.Aggregates.Players;

/// <summary>UC12: lasts 30 days; can be extended by 30. No failure mode -> plain factory.</summary>
public sealed class VipStatus : ValueObject
{
    private VipStatus() { }
    public DateOnly EndDate { get; }
    private VipStatus(DateOnly endDate) => EndDate = endDate;

    public static VipStatus GrantedOn(DateOnly today) => new(today.AddDays(30)); // UC12 S1
    public VipStatus Extend(int days) => new(EndDate.AddDays(days));             // UC12 S3
    public bool IsActiveOn(DateOnly date) => date <= EndDate;

    protected override IEnumerable<object?> GetEqualityComponents() { yield return EndDate; }
}
