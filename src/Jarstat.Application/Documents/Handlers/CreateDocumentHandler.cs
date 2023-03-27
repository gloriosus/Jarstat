using Jarstat.Application.Abstractions;
using Jarstat.Application.Commands;
using Jarstat.Application.Services;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, Result<Document?>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDocumentHandler(
        IDocumentRepository documentRepository,
        IFolderRepository folderRepository,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _folderRepository = folderRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Document?>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentCreationResult = await CreateAsync(request);
        if (documentCreationResult.IsFailure)
            return documentCreationResult!;

        var result = await _documentRepository.CreateAsync(documentCreationResult.Value!);

        var downgradeDocumentsResult = await DowngradeDocumentsAbove(documentCreationResult.Value!, request);
        if (downgradeDocumentsResult.IsFailure)
            return downgradeDocumentsResult!;

        var downgradeFoldersResult = await DowngradeFoldersAbove(request);
        if (downgradeFoldersResult.IsFailure)
            return downgradeFoldersResult!;

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

    private async Task<Result<Document?>> CreateAsync(CreateDocumentCommand request)
    {
        var folder = await _folderRepository.GetByIdAsync(request.FolderId);
        if (folder is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.FolderId), typeof(Guid).ToString(), request.FolderId.ToString()));

        var creator = await _userManager.FindByIdAsync(request.CreatorId.ToString());
        if (creator is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.CreatorId), typeof(Guid).ToString(), request.CreatorId.ToString()));

        var documentCreationResult = Document.Create(
            request.DisplayName,
            request.FileName,
            folder,
            request.Description,
            creator,
            request.FileId);

        return documentCreationResult;
    }

    private async Task<Result<Document?>> DowngradeDocumentsAbove(Document createdDocument, CreateDocumentCommand request)
    {
        var documentsInFolder = (await _documentRepository.GetByFolderId(request.FolderId))
                                                          .Where(d => d.Id != createdDocument.Id);
        foreach (var document in documentsInFolder)
        {
            var documentOrderingResult = document.ChangeSortOrder(document.SortOrder / 2);
            if (documentOrderingResult.IsFailure)
                return documentOrderingResult;

            var orderedDocument = documentOrderingResult.Value!;
            _documentRepository.Update(orderedDocument);
        }

        return Result<Document?>.Success(default);
    }

    private async Task<Result<Document?>> DowngradeFoldersAbove(CreateDocumentCommand request)
    {
        var foldersInFolder = await _folderRepository.GetByParentId(request.FolderId);

        foreach (var folder in foldersInFolder)
        {
            var folderOrderingResult = folder.ChangeSortOrder(folder.SortOrder / 2);
            if (folderOrderingResult.IsFailure)
                // TODO: Change later
                return Result<Document?>.Failure(folderOrderingResult.Error);

            var orderedFolder = folderOrderingResult.Value!;
            _folderRepository.Update(orderedFolder);
        }

        return Result<Document?>.Success(default);
    }
}
