using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, Result<Document>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentHandler(IDocumentRepository documentRepository, IFileRepository fileRepository, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _fileRepository = fileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Document>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        if (document is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString());

        if (document.FileId is not null)
        {
            var file = await _fileRepository.GetByIdAsync(document.FileId);
            if (file is not null)
                _fileRepository.Delete(file);
        }

        var result = _documentRepository.Delete(document);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return DomainErrors.Exception.WithParameters(ex.InnerException?.Message!);
        }

        return result;
    }
}
