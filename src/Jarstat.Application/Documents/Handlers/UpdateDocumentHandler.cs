using Jarstat.Application.Abstractions;
using Jarstat.Application.Commands;
using Jarstat.Application.Services;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, Result<Document?>>
{
    private readonly AbstractDocumentUpdatingService _documentUpdatingService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentHandler(
        AbstractDocumentUpdatingService documentUpdatingService, 
        IDocumentRepository documentRepository, 
        IUnitOfWork unitOfWork)
    {
        _documentUpdatingService = documentUpdatingService;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Document?>> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentUpdatingResult = await _documentUpdatingService.UpdateAsync(request);
        if (documentUpdatingResult.IsFailure)
            return documentUpdatingResult;

        var result = _documentRepository.Update(documentUpdatingResult.Value!);

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
