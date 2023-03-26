using Jarstat.Client.Enums;
using Jarstat.Domain.Shared;

namespace Jarstat.Client.Requests;

public class ReorderItemRequest
{
    public Guid ItemId { get; set; }
    public Guid TargetItemId { get; set; }
    public DropPosition DropPosition { get; set; }
}
