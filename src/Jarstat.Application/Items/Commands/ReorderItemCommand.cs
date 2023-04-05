using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class ReorderItemCommand : IRequest<Result<Item>>
{
    public Guid ItemId { get; set; }
    public Guid TargetItemId { get; set; }
    public DropPosition DropPosition { get; set; }
}
