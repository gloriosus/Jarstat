namespace Jarstat.Client.Requests;

public class SearchDocumentsRequest
{
    public string? DisplayName { get; set; }
    public Guid[] ParentIds { get; set; } = Array.Empty<Guid>();
    public int Skip { get; set; }
    public int Take { get; set; }
}
