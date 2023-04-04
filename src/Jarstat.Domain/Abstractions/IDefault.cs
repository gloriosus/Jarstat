namespace Jarstat.Domain.Abstractions;

public interface IDefault<T>
{
    static abstract T? Default { get; }
}
