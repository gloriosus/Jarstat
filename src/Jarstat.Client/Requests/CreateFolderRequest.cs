namespace Jarstat.Client.Requests;

public class CreateFolderRequest
{
    public string DisplayName { get; set; } = null!;
    public string VirtualPath { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Guid CreatorId { get; set; }
}
