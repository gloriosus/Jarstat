using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class CreateFolderHandler : IRequestHandler<CreateFolderCommand, Result<Folder?>>
{
    private readonly IFolderRepository _folderRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFolderHandler(
        IFolderRepository folderRepository, 
        UserManager<User> userManager, 
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Folder?>> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        var creator = await _userManager.FindByIdAsync(request.CreatorId.ToString());
        if (creator is null)
            return Result<Folder?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.CreatorId), typeof(Guid).ToString(), request.CreatorId.ToString()));

        Folder? parent = null;
        if (request.ParentId is not null)
            parent = await _folderRepository.GetByIdAsync((Guid)request.ParentId);

        var folderCreationResult = Folder.Create(request.DisplayName, request.VirtualPath, parent, creator);
        if (folderCreationResult.IsFailure)
            return folderCreationResult;

        var result = await _folderRepository.CreateAsync(folderCreationResult.Value!);

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
