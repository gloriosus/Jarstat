namespace Jarstat.Client.Requests;

public class CopyFileRequest
{
    public string FileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public Guid CreatorId { get; set; }
}
