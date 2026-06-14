using System.Text.RegularExpressions;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.Players;

/// <summary>UC5: a @via.dk address whose local part is 3-4 letters OR exactly 6 digits. Stored lower-case.</summary>
public sealed partial class ViaEmail : ValueObject
{
    private ViaEmail () {}
    public string Value { get; }
    private ViaEmail(string value) => Value = value;

    public static Result<ViaEmail> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new ResultError("email.empty", "Email cannot be empty.");                       // UC5 F3
        var normalized = email.Trim().ToLowerInvariant();
        var parts = normalized.Split('@');
        if (parts.Length != 2 || parts[1] != "via.dk")
            return new ResultError("email.domain", "Only people with a VIA email (@via.dk) can register."); // F1
        if (!LocalPart().IsMatch(parts[0]))
            return new ResultError("email.format", "The email must be 3-4 letters or 6 digits before @via.dk."); // F2
        return new ViaEmail(normalized);
    }

    /// <summary>The VIA id, i.e. the part before @via.dk (e.g. "trmo").</summary>
    public string ViaId() => Value.Split('@')[0];

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;

    [GeneratedRegex(@"^([a-z]{3,4}|\d{6})$")]
    private static partial Regex LocalPart();
}
