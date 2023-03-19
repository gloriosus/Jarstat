using Jarstat.Application.Abstractions;
using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Services;

public class DocumentCreationService : AbstractDocumentCreationService
{
    private readonly IFolderRepository _folderRepository;
    private readonly UserManager<User> _userManager;

    public DocumentCreationService(
        IFolderRepository folderRepository,
        UserManager<User> userManager)
    {
        _folderRepository = folderRepository;
        _userManager = userManager;
    }

    public override async Task<Result<Document?>> CreateAsync(CreateDocumentCommand request)
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
}
