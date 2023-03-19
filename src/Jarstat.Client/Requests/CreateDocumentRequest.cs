namespace Jarstat.Client.Requests;

public class CreateDocumentRequest
{
    public string DisplayName { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid FolderId { get; set; }
    public string? Description { get; set; }
    public Guid CreatorId { get; set; }
    public Guid? FileId { get; set; }
}
