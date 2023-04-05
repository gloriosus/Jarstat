using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetAllDocumentsHandler : IRequestHandler<GetAllDocumentsQuery, Result<Assortment<Document>>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetAllDocumentsHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<Assortment<Document>>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetAllAsync();
        return result;
    }
}
