namespace ViaPadel.Core.Tools.OperationResult;

public static class ResultExtensions
{
    public static bool TryValue<T>(this Result<T> result, ICollection<ResultError> sink, out T value)
    {
        if (result is Success<T> s) { value = s.Value; return true; }
        if (result is Failure<T> f) foreach (var e in f.Errors) sink.Add(e);
        value = default!;
        return false;
    }
}