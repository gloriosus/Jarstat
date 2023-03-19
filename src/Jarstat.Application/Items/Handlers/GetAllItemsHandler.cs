using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetAllItemsHandler : IRequestHandler<GetAllItemsQuery, Result<List<Item>>>
{
    private readonly IItemRepository _itemRepository;

    public GetAllItemsHandler(IItemRepository itemRepository) => _itemRepository = itemRepository;

    public async Task<Result<List<Item>>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        var result = await _itemRepository.GetAllAsync();
        return result;
    }
}
