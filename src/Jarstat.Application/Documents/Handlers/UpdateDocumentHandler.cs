using Jarstat.Application.Commands;
using Jarstat.Application.Services;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, Result<Document>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentHandler(
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

    public async Task<Result<Document>> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var documentUpdatingResult = await UpdateAsync(request);
        if (documentUpdatingResult.IsFailure)
            return documentUpdatingResult;

        var result = _documentRepository.Update(documentUpdatingResult.Value!);

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

    public async Task<Result<Document>> UpdateAsync(UpdateDocumentCommand request)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        if (document is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString());

        var folder = await _folderRepository.GetByIdAsync(request.FolderId);
        if (folder is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.FolderId), typeof(Guid).ToString(), request.FolderId.ToString());

        var lastUpdater = await _userManager.FindByIdAsync(request.LastUpdaterId.ToString());
        if (lastUpdater is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.LastUpdaterId), typeof(Guid).ToString(), request.LastUpdaterId.ToString());

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
