namespace Jarstat.Domain.Records;

public sealed record Item(Guid Id, string DisplayName, Guid? ParentId, string Type, DateTime DateTimeCreated, DateTime DateTimeUpdated, double SortOrder);
