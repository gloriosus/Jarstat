namespace Jarstat.Domain.Records;

public record SearchResult<T>(List<T> Items, int Count);
