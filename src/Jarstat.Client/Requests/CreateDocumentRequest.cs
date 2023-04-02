namespace Jarstat.Client.Requests;

public class CreateDocumentRequest
{
    public string DisplayName { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid FolderId { get; set; }
    public string? Description { get; set; }
    public Guid CreatorId { get; set; }
    public Guid? FileId { get; set; }

    public void Clear()
    {
        DisplayName = string.Empty;
        FileName = string.Empty;
        FolderId = Guid.Empty;
        Description = null;
        CreatorId = Guid.Empty;
        FileId = null;
    }
}
