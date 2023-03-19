using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Jarstat.Application.Handlers;

public class CopyFileHandler : IRequestHandler<CopyFileCommand, Guid?>
{
    private readonly IFileRepository _fileRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public CopyFileHandler(
        IFileRepository fileRepository,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration) 
    {
        _fileRepository = fileRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<Guid?> Handle(CopyFileCommand request, CancellationToken cancellationToken)
    {
        var creator = await _userManager.FindByIdAsync(request.CreatorId.ToString());
        if (creator is null)
            return null;

        string storagePath = _configuration["TempStoragePath"] ?? "C:\\temp_storage";
        string fileExtension = Path.GetExtension(request.FileName);
        string fileName = $"{request.StoredFileName}{fileExtension}";
        string filePath = Path.Combine(storagePath, fileName);

        await using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[fs.Length];
        await fs.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        var fileCreationResult = Domain.Entities.File.Create(buffer, creator);
        if (fileCreationResult.IsFailure)
            return null;

        var result = await _fileRepository.CreateAsync(fileCreationResult.Value!);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return null;
        }

        return result?.Id;
    }
}
