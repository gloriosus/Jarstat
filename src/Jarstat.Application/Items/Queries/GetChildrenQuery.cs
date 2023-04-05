using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetChildrenQuery : IRequest<Result<Assortment<Item>>>
{
    public GetChildrenQuery(Guid parentId) => ParentId = parentId;

    public Guid ParentId { get; }
}
