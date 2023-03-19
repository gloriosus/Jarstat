using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class UpdateFolderHandler : IRequestHandler<UpdateFolderCommand, Result<Folder?>>
{
    private readonly IFolderRepository _folderRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFolderHandler(
        IFolderRepository folderRepository,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Folder?>> Handle(UpdateFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepository.GetByIdAsync(request.Id);
        if (folder is null)
            return Result<Folder?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString()));

        Folder? parent = null;
        if (request.ParentId is not null)
            parent = await _folderRepository.GetByIdAsync((Guid)request.ParentId);

        var lastUpdater = await _userManager.FindByIdAsync(request.LastUpdaterId.ToString());
        if (lastUpdater is null)
            return Result<Folder?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.LastUpdaterId), typeof(Guid).ToString(), request.LastUpdaterId.ToString()));

        var folderUpdatingResult = folder.Update(request.DisplayName, request.VirtualPath, parent, lastUpdater);
        if (folderUpdatingResult.IsFailure)
            return folderUpdatingResult;

        var result = _folderRepository.Update(folderUpdatingResult.Value!);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Folder?>.Failure(DomainErrors.Exception
                .WithParameters(ex.InnerException?.Message!));
        }

        return result;
    }
}
