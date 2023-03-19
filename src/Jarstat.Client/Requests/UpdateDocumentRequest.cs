using System.Text.Json.Serialization;

namespace Jarstat.Client.Requests;

public class UpdateDocumentRequest
{
    [JsonPropertyName("id")]
    public Guid DocumentId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid FolderId { get; set; }
    public string? Description { get; set; }
    public Guid LastUpdaterId { get; set; }
    public Guid? FileId { get; set; }
}
