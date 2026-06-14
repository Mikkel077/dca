namespace ViaPadel.Core.Domain.Contracts;

/// <summary>
/// "Domain Contract" (session 5) for the current moment. Inject a fake in unit tests
/// so the future/past rules (UC2.F2, UC4.F2, UC6.F19, UC7, UC8, UC15.F4 ...) are deterministic.
/// </summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateOnly Today => DateOnly.FromDateTime(Now);
    TimeOnly CurrentTime => TimeOnly.FromDateTime(Now);
}
