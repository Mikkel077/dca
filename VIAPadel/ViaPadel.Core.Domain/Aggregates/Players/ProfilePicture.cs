using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.Players;

public sealed class ProfilePicture : ValueObject
{
    private ProfilePicture() { }
    public Uri Uri { get; }
    private ProfilePicture(Uri uri) => Uri = uri;

    public static Result<ProfilePicture> Create(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            return new ResultError("picture.format", "The profile picture URL has an incorrect format."); // F4
        return new ProfilePicture(parsed);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Uri; }
    public override string ToString() => Uri.ToString();
}
