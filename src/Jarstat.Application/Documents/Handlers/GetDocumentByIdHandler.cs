using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, Result<Document>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentByIdHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<Document>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetByIdAsync(request.Id);
        if (result is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString());

        return result;
    }
}
