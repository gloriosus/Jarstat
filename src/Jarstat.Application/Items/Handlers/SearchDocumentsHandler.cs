using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class SearchDocumentsHandler : IRequestHandler<SearchDocumentsCommand, Result<SearchResult<Item>>>
{
    private readonly IItemRepository _itemRepository;

    public SearchDocumentsHandler(IItemRepository itemRepository) => _itemRepository = itemRepository;

    public async Task<Result<SearchResult<Item>>> Handle(SearchDocumentsCommand request, CancellationToken cancellationToken)
    {
        if (request.Skip < 0)
            return Result<SearchResult<Item>>.Failure(DomainErrors.ArgumentNotAcceptableValue
                .WithParameters(nameof(request.Skip), typeof(int).ToString(), request.Skip.ToString()));

        if (request.Take <= 0)
            return Result<SearchResult<Item>>.Failure(DomainErrors.ArgumentNotAcceptableValue
                .WithParameters(nameof(request.Take), typeof(int).ToString(), request.Take.ToString()));

        var result = await _itemRepository.SearchDocuments(request.DisplayName, request.ParentIds, request.Skip, request.Take);

        return result;
    }
}
