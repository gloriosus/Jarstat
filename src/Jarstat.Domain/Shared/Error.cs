using Jarstat.Domain.Primitives;
using System.Text.Json.Serialization;

namespace Jarstat.Domain.Shared;

public class Error : IEquatable<Error>
{
    public static readonly Error None = new Error(string.Empty, string.Empty);

    [JsonConstructor]
    public Error(string code, string message, params string[] parameters)
    {
        Code = code;
        Message = message;
        Parameters = parameters;
    }

    public string Code { get; }
    public string Message { get; }
    public string[] Parameters { get; private set; }

    public Error WithParameters(params string[] parameters) => new Error(Code, Message, parameters);

    public static implicit operator string(Error error) => error.Code;

    public static bool operator ==(Error? first, Error? second)
    {
        return first is not null && second is not null && first.Equals(second);
    }

    public static bool operator !=(Error? first, Error? second)
    {
        return !(first == second);
    }

    public bool Equals(Error? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return other.Code == Code && other.Message == Message;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        if (obj is not Error error)
            return false;

        return error.Code == Code && error.Message == Message;
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode() * 79;
    }
}
