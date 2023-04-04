using Jarstat.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace Jarstat.Client.Responses;

public record ItemResponse : IDefault<ItemResponse>
{
    [JsonConstructor]
    public ItemResponse(
        Guid itemId, 
        string displayName, 
        Guid? parentId, 
        string type,
        double sortOrder)
    {
        ItemId = itemId;
        DisplayName = displayName;
        ParentId = parentId;
        Type = type;
        SortOrder = sortOrder;
    }

    public static ItemResponse? Default => null;

    [JsonPropertyName("id")]
    public Guid ItemId { get; init; }
    public string DisplayName { get; init; } = null!;
    public Guid? ParentId { get; init; }
    public string Type { get; init; } = null!;
    public DateTime DateTimeCreated { get; init; }
    public DateTime DateTimeUpdated { get; init; }
    public double SortOrder { get; init; }

    [JsonIgnore]
    public Assortment<ItemResponse> Children { get; set; } = new();
}
