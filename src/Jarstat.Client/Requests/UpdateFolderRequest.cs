using System.Text.Json.Serialization;

namespace Jarstat.Client.Requests;

public class UpdateFolderRequest
{
    [JsonPropertyName("id")]
    public Guid FolderId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string VirtualPath { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Guid LastUpdaterId { get; set; }
}
