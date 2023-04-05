using Jarstat.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace Jarstat.Domain.Shared;

public class Result<T> where T : IDefault<T>
{
    // TODO: make the constructor protected
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
    public static implicit operator Result<T>(Error error) => Failure(error);

    public static Result<T> Success(T value) => new Result<T>(value, true, Error.None);
    public static Result<T> Failure(Error error) => new Result<T>(T.Default, false, error);

    public Result<TDestination> AsResult<TDestination>()
        where TDestination : IDefault<TDestination>
    {
        return new Result<TDestination>(TDestination.Default, IsSuccess, Error);
    }

    public Result<TDestination> AsResult<TDestination>(TDestination value)
        where TDestination : IDefault<TDestination>
    {
        return new Result<TDestination>(value, IsSuccess, Error);
    }
}
