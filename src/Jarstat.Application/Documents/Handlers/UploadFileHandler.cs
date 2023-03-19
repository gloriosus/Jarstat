using Jarstat.Application.Commands;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Jarstat.Application.Handlers;

public class UploadFileHandler : IRequestHandler<UploadFileCommand, UploadResult>
{
    private readonly IConfiguration _configuration;

    public UploadFileHandler(IConfiguration configuration) => _configuration = configuration;

    public async Task<UploadResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        string storagePath = _configuration["TempStoragePath"] ?? "C:\\temp_storage";
        string fileExtension = Path.GetExtension(request.File.FileName);
        string fileName = $"{id}{fileExtension}";
        string filePath = Path.Combine(storagePath, fileName);

        if (!Directory.Exists(storagePath))
            Directory.CreateDirectory(storagePath);

        await using FileStream fs = new FileStream(filePath, FileMode.Create);
        await request.File.CopyToAsync(fs);

        if (fs.Length == 0)
            return new UploadResult
            {
                FileName = null,
                StoredFileName = null
            };

        return new UploadResult
        {
            FileName = request.File.FileName,
            StoredFileName = id.ToString()
        };
    }
}
