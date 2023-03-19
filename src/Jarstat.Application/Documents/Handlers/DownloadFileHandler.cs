using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class DownloadFileHandler : IRequestHandler<DownloadFileQuery, Result<Document?>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileRepository _fileRepository;

    public DownloadFileHandler(IDocumentRepository documentRepository, IFileRepository fileRepository)
    {
        _documentRepository = documentRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<Document?>> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        if (document is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString()));

        var file = await _fileRepository.GetByIdAsync(document.FileId);
        if (file is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(document.FileId), typeof(Guid?).ToString(), document.FileId?.ToString()!));

        var result = document.WithFile(file);

        return result;
    }
}
