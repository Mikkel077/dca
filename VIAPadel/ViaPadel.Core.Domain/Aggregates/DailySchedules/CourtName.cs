using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

/// <summary>UC3: starts with S or D, ends with a number 1-10, length 2-3. Stored capitalized ("s1" -> "S1").</summary>
public sealed class CourtName : ValueObject
{
    private CourtName () {}
    public string Value { get; }
    private CourtName(string value) => Value = value;

    public static Result<CourtName> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new ResultError("court.empty", "A court name is required.");
        var name = raw.Trim();
        if (name.Length is not (2 or 3))
            return new ResultError("court.length", "A court name must be 2 or 3 characters.");            // F5
        var letter = char.ToUpperInvariant(name[0]);
        if (letter is not ('S' or 'D'))
            return new ResultError("court.letter", "A court name must start with S or D.");               // F2
        if (!int.TryParse(name[1..], out var number) || number is < 1 or > 10)
            return new ResultError("court.number", "A court name must end with a number between 1 and 10."); // F4
        return new CourtName($"{letter}{number}");                                                        // S1 (capitalized)
    }
    public static CourtName From(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
