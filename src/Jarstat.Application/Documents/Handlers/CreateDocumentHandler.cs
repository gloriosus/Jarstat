using Jarstat.Application.Abstractions;
using Jarstat.Application.Commands;
using Jarstat.Application.Services;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, Result<Document?>>
{
    private readonly AbstractDocumentCreationService _documentCreationService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDocumentHandler(
        AbstractDocumentCreationService documentCreationService,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentCreationService = documentCreationService;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Document?>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentCreationResult = await _documentCreationService.CreateAsync(request);

        if (documentCreationResult.IsFailure)
            return documentCreationResult!;

        var result = await _documentRepository.CreateAsync(documentCreationResult.Value!);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Document?>.Failure(DomainErrors.Exception
                .WithParameters(ex.InnerException?.Message!));
        }

        return result;
    }
}
