using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Jarstat.Application.Handlers;

public class UploadFileHandler : IRequestHandler<UploadFileCommand, Result<UploadValue>>
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

    public async Task<Result<UploadValue>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var creator = await _userManager.FindByIdAsync(request.CreatorId.ToString());
        if (creator is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.CreatorId), typeof(Guid).ToString(), request.CreatorId.ToString());

        await using Stream stream = request.File.OpenReadStream();
        if (stream.Length == 0)
            return DomainErrors.StreamLengthEqualsZero
                .WithParameters(nameof(stream), typeof(Stream).ToString());

        var buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        var fileCreationResult = Domain.Entities.File.Create(buffer, creator);
        if (fileCreationResult.IsFailure)
            return fileCreationResult.AsResult<UploadValue>();

        var result = await _fileRepository.CreateAsync(fileCreationResult.Value!);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return DomainErrors.Exception;
        }

        return new UploadValue(request.File.FileName, result?.Id);
    }
}
