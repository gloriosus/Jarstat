using Jarstat.Domain.Abstractions;

namespace Jarstat.Domain.Records;

public sealed record SearchValue<T>(List<T> Items, int Count) : IDefault<SearchValue<T>>
{
    public static SearchValue<T>? Empty = new SearchValue<T>(new List<T>(), 0); 
    public static SearchValue<T>? Default => Empty;
}
