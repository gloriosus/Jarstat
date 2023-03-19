using Jarstat.Application.Abstractions;
using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Services;

public class DocumentUpdatingService : AbstractDocumentUpdatingService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly UserManager<User> _userManager;

    public DocumentUpdatingService(
        IDocumentRepository documentRepository,
        IFolderRepository folderRepository,
        UserManager<User> userManager)
    {
        _documentRepository = documentRepository;
        _folderRepository = folderRepository;
        _userManager = userManager;
    }

    public override async Task<Result<Document?>> UpdateAsync(UpdateDocumentCommand request)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        if (document is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString()));
        
        var folder = await _folderRepository.GetByIdAsync(request.FolderId);
        if (folder is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.FolderId), typeof(Guid).ToString(), request.FolderId.ToString()));

        var lastUpdater = await _userManager.FindByIdAsync(request.LastUpdaterId.ToString());
        if (lastUpdater is null)
            return Result<Document?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.LastUpdaterId), typeof(Guid).ToString(), request.LastUpdaterId.ToString()));

        var documentUpdatingResult = document.Update(
            request.DisplayName,
            request.FileName,
            folder,
            request.Description,
            lastUpdater,
            request.FileId);

        return documentUpdatingResult;
    }
}
