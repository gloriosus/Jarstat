using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Jarstat.Application.Handlers;

public class UploadFileHandler : IRequestHandler<UploadFileCommand, UploadResult>
{
    private readonly IFileRepository _fileRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UploadFileHandler(
        IFileRepository fileRepository,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _fileRepository = fileRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var creator = await _userManager.FindByIdAsync(request.CreatorId.ToString());
        if (creator is null)
            return UploadResult.Empty;

        await using Stream stream = request.File.OpenReadStream();
        if (stream.Length == 0)
            return UploadResult.Empty;

        var buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        var fileCreationResult = Domain.Entities.File.Create(buffer, creator);
        if (fileCreationResult.IsFailure)
            return UploadResult.Empty;

        var result = await _fileRepository.CreateAsync(fileCreationResult.Value!);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return UploadResult.Empty;
        }

        return new UploadResult(request.File.FileName, result?.Id);
    }
}
