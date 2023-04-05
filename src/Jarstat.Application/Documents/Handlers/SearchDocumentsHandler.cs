using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class SearchDocumentsHandler : IRequestHandler<SearchDocumentsCommand, Result<SearchValue<Document>>>
{
    private readonly IDocumentRepository _documentRepository;

    public SearchDocumentsHandler(IDocumentRepository documentRepository) => _documentRepository = documentRepository;

    public async Task<Result<SearchValue<Document>>> Handle(SearchDocumentsCommand request, CancellationToken cancellationToken)
    {
        if (request.Skip < 0)
            return DomainErrors.ArgumentNotAcceptableValue
                .WithParameters(nameof(request.Skip), typeof(int).ToString(), request.Skip.ToString());

        if (request.Take <= 0)
            return DomainErrors.ArgumentNotAcceptableValue
                .WithParameters(nameof(request.Take), typeof(int).ToString(), request.Take.ToString());

        var result = await _documentRepository.SearchDocuments(request.DisplayName, request.ParentIds, request.Skip, request.Take);

        return result;
    }
}
