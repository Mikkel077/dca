using System.Text.RegularExpressions;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.Players;

/// <summary>UC5: first/last name are 2-25 letters, no symbols/spaces. Stored capitalized.
/// Accumulates both name errors (F5 + F6) since they are independent.</summary>
public sealed partial class PersonName : ValueObject
{
    private PersonName() { }
    public string FirstName { get; }
    public string LastName { get; }
    private PersonName(string first, string last) { FirstName = first; LastName = last; }

    public static Result<PersonName> Create(string? firstName, string? lastName)
    {
        var errors = new List<ResultError>();
        if (!IsValid(firstName))
            errors.Add(new ResultError("name.first", "First name must be 2-25 letters with no symbols or spaces.")); // F5
        if (!IsValid(lastName))
            errors.Add(new ResultError("name.last", "Last name must be 2-25 letters with no symbols or spaces."));   // F6
        if (errors.Count > 0) return new Failure<PersonName>(errors);
        return new PersonName(Normalize(firstName!), Normalize(lastName!));
    }

    private static bool IsValid(string? raw) => raw is not null && Letters().IsMatch(raw.Trim());
    private static string Normalize(string raw)
    {
        var t = raw.Trim();
        return char.ToUpperInvariant(t[0]) + t[1..].ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return FirstName; yield return LastName; }
    public override string ToString() => $"{FirstName} {LastName}";

    [GeneratedRegex(@"^[a-zA-Z]{2,25}$")]
    private static partial Regex Letters();
}
