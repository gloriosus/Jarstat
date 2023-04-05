using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetRootsHandler : IRequestHandler<GetRootsQuery, Result<Assortment<Item>>>
{
    private readonly IItemRepository _itemRepository;

    public GetRootsHandler(IItemRepository itemRepository) => _itemRepository = itemRepository;

    public async Task<Result<Assortment<Item>>> Handle(GetRootsQuery request, CancellationToken cancellationToken)
    {
        var result = await _itemRepository.GetRootsAsync();
        return result;
    }
}
