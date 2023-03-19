using System.Text.Json.Serialization;

namespace Jarstat.Client.Responses;

public record FolderResponse
{
    [JsonConstructor]
    public FolderResponse(
        Guid folderId, 
        string displayName, 
        string virtualPath, 
        Guid? parentId, 
        FolderResponse? parent, 
        DateTime dateTimeCreated, 
        DateTime dateTimeUpdated, 
        UserResponse creator, 
        UserResponse lastUpdater,
        double sortOrder)
    {
        FolderId = folderId;
        DisplayName = displayName;
        VirtualPath = virtualPath;
        ParentId = parentId;
        Parent = parent;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        Creator = creator;
        LastUpdater = lastUpdater;
        SortOrder = sortOrder;
    }

    [JsonPropertyName("id")]
    public Guid FolderId { get; init; }
    public string DisplayName { get; init; } = null!;
    public string VirtualPath { get; init; } = null!;
    public Guid? ParentId { get; init; }
    public FolderResponse? Parent { get; init; }
    public DateTime DateTimeCreated { get; init; }
    public DateTime DateTimeUpdated { get; init; }
    public UserResponse Creator { get; init; } = null!;
    public UserResponse LastUpdater { get; init; } = null!;
    public double SortOrder { get; init; }

    [JsonIgnore]
    public List<FolderResponse> Children { get; set; } = new();
}
