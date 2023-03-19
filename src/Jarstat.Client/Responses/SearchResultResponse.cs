namespace Jarstat.Client.Responses;

public record SearchResultResponse<T>(List<T> Items, int Count);
