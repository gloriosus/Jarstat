using Jarstat.Domain.Abstractions;

namespace Jarstat.Domain.Records;

public sealed record Item(
    Guid Id, 
    string DisplayName, 
    Guid? ParentId, 
    string Type, 
    DateTime DateTimeCreated,
    DateTime DateTimeUpdated,
    double SortOrder) 
    : IDefault<Item>
{
    public static Item? Default => null;
}
