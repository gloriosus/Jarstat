using Jarstat.Domain.Abstractions;

namespace Jarstat.Client.Responses;

public record SearchResponse<T>(List<T> Items, int Count) : IDefault<SearchResponse<T>>
{
    public static SearchResponse<T>? Empty = new SearchResponse<T>(new List<T>(), 0);
    public static SearchResponse<T>? Default => Empty;
}
