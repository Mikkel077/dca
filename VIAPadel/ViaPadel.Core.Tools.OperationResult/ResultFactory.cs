namespace ViaPadel.Core.Tools.OperationResult;

/// <summary>
/// Convenience factory so handlers don't have to write
/// `new Succes&lt;None&gt;(new None())` / `new Failure&lt;None&gt;([...])` everywhere.
/// Everything is returned as the base <see cref="Result"/> so it fits
/// <c>Task&lt;Result&gt;</c> from <c>ICommandHandler</c>.
/// </summary>
public static class ResultFactory
{
    // Success without a payload (the common case for commands).
    public static Result Ok() => new Success<None>(new None());

    // Success that carries a value (e.g. the Id of a newly created entity).
    public static Result Ok<T>(T value) => new Success<T>(value);

    // Failure for a payload-less handler.
    public static Result Error(params ResultError[] errors) => new Failure<None>(errors);

    // Failure for a handler whose success type is T (keeps the whole thing a Result<T>).
    public static Result Error<T>(params ResultError[] errors) => new Failure<T>(errors);
}
