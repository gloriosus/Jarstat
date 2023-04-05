using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class CreateFolderHandler : IRequestHandler<CreateFolderCommand, Result<Folder>>
{
    private readonly IFolderRepository _folderRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFolderHandler(
        IFolderRepository folderRepository,
        IDocumentRepository documentRepository,
        UserManager<User> userManager, 
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Folder>> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        var folderCreationResult = await CreateAsync(request);
        if (folderCreationResult.IsFailure)
            return folderCreationResult;

        var result = await _folderRepository.CreateAsync(folderCreationResult.Value!);

        var downgradeFoldersResult = await DowngradeFoldersAbove(folderCreationResult.Value!, request);
        if (downgradeFoldersResult.IsFailure)
            return downgradeFoldersResult!;

        var downgradeDocumentsResult = await DowngradeDocumentsAbove(request);
        if (downgradeDocumentsResult.IsFailure)
            return downgradeDocumentsResult!;

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

    private async Task<Result<Folder>> CreateAsync(CreateFolderCommand request)
    {
        var creator = await _userManager.FindByIdAsync(request.CreatorId.ToString());
        if (creator is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.CreatorId), typeof(Guid).ToString(), request.CreatorId.ToString());

        Folder? parent = null;
        if (request.ParentId is not null)
            parent = await _folderRepository.GetByIdAsync((Guid)request.ParentId);

        return Folder.Create(request.DisplayName, request.VirtualPath, parent, creator);
    }

    private async Task<Result<Folder>> DowngradeDocumentsAbove(CreateFolderCommand request)
    {
        var documentsInFolder = await _documentRepository.GetByFolderId((Guid)request.ParentId!);

        foreach (var document in documentsInFolder)
        {
            var documentOrderingResult = document.ChangeSortOrder(document.SortOrder / 2);
            if (documentOrderingResult.IsFailure)
                return documentOrderingResult.AsResult<Folder>();

            var orderedDocument = documentOrderingResult.Value!;
            _documentRepository.Update(orderedDocument);
        }

        return Folder.Default;
    }

    private async Task<Result<Folder>> DowngradeFoldersAbove(Folder createdFolder, CreateFolderCommand request)
    {
        var foldersInFolder = (await _folderRepository.GetByParentId(request.ParentId))
                                                      .Where(f => f.Id != createdFolder.Id);

        foreach (var folder in foldersInFolder)
        {
            var folderOrderingResult = folder.ChangeSortOrder(folder.SortOrder / 2);
            if (folderOrderingResult.IsFailure)
                return folderOrderingResult;

            var orderedFolder = folderOrderingResult.Value!;
            _folderRepository.Update(orderedFolder);
        }

        return Folder.Default;
    }
}
