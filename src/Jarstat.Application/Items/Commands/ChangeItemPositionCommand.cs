using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class ChangeItemPositionCommand : IRequest<bool>
{
    public Guid ItemId { get; set; }
    public Guid TargetItemId { get; set; }
    public DropPosition DropPosition { get; set; }
    //public string ItemType { get; set; } = null!;
    //public Guid ParentId { get; set; }
    //public double SortOrder { get; set; }
}
