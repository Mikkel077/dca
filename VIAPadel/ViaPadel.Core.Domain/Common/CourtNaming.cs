using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.Common;

/// <summary>
/// Court-name rules from UC3.S1:
/// starts with S/s or D/d, ends with a number 1..10, total length 2 or 3, stored capitalized.
/// Returns the specific error so UC3.F2 (letter) / F4 (number) / F5 (length) stay distinguishable.
/// </summary>
public static class CourtNaming
{
    public static bool TryNormalize(string? raw, out string normalized, out ResultError? error)
    {
        normalized = string.Empty;
        error = null;

        var name = raw?.Trim() ?? string.Empty;

        if (name.Length is not (2 or 3)) { error = Errors.Court.InvalidLength; return false; }

        var first = char.ToUpperInvariant(name[0]);
        if (first is not ('S' or 'D')) { error = Errors.Court.InvalidStartingLetter; return false; }

        var numberPart = name[1..];
        if (!int.TryParse(numberPart, out var number) || number is < 1 or > 10)
        {
            error = Errors.Court.InvalidEndingNumber;
            return false;
        }

        normalized = $"{first}{number}";
        return true;
    }
}
