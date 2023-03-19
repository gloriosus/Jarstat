namespace Jarstat.Client.Requests;

public class ChangeItemPositionRequest
{
    public Guid ItemId { get; set; }
    public Guid TargetItemId { get; set; }
}
