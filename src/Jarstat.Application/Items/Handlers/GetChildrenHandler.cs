using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetChildrenHandler : IRequestHandler<GetChildrenQuery, Result<List<Item>>>
{
    private readonly IItemRepository _itemRepository;

    public GetChildrenHandler(IItemRepository itemRepository) => _itemRepository = itemRepository;

    public async Task<Result<List<Item>>> Handle(GetChildrenQuery request, CancellationToken cancellationToken)
    {
        var result = await _itemRepository.GetChildrenAsync(request.ParentId);
        return result;
    }
}
