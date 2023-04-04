using Jarstat.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace Jarstat.Client.Responses;

public record DocumentResponse : IDefault<DocumentResponse>
{
    [JsonConstructor]
    public DocumentResponse(
        Guid documentId,
        string displayName,
        string fileName,
        Guid folderId,
        FolderResponse folder,
        string? description,
        Guid? fileId,
        DateTime dateTimeCreated,
        DateTime dateTimeUpdated,
        UserResponse creator,
        UserResponse lastUpdater,
        double sortOrder)
    {
        DocumentId = documentId;
        DisplayName = displayName;
        FileName = fileName;
        FolderId = folderId;
        Folder = folder;
        Description = description;
        FileId = fileId;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        Creator = creator;
        LastUpdater = lastUpdater;
        SortOrder = sortOrder;
    }

    public static DocumentResponse? Default => null;

    [JsonPropertyName("id")]
    public Guid DocumentId { get; init; }
    public string DisplayName { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public Guid FolderId { get; init; }
    public FolderResponse Folder { get; init; } = null!;
    public string? Description { get; init; }
    public Guid? FileId { get; init; }
    public DateTime DateTimeCreated { get; init; }
    public DateTime DateTimeUpdated { get; init; }
    public UserResponse Creator { get; init; } = null!;
    public UserResponse LastUpdater { get; init; } = null!;
    public double SortOrder { get; init; }
}
