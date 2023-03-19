using System.Text.Json.Serialization;

namespace Jarstat.Domain.Shared;

public class Result<T>
{
    [JsonConstructor]
    public Result(T? value, bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException(); 

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public T? Value { get; private set; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static implicit operator Result<T>(T value) => Success(value);

    public static Result<T> Success(T value) => new Result<T>(value, true, Error.None);
    public static Result<T> Failure(Error error) => new Result<T>(default(T), false, error);
}
