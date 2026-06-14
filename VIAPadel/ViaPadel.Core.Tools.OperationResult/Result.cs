namespace ViaPadel.Core.Tools.OperationResult;

public abstract record Result
{
    public bool IsFailure => this is Failure;
    public bool IsSuccess => !IsFailure;
    public IEnumerable<ResultError> Errors =>
        this is Failure f ? f.Errors : [];
};

public abstract record Failure : Result;

public record Success<T>(T Value) : Result<T>;

public record Failure<T>(IEnumerable<ResultError> Errors) : Result<T>;

public record ResultError(string code, string message);

public record None;

public abstract record Result<T> : Result
{
    public static implicit operator Result<T>(ResultError error) => new Failure<T>([error]);

    public static implicit operator Result<T>(T value) => new Success<T>(value);
}

