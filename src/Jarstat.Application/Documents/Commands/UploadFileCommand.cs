using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Jarstat.Application.Commands;

public class UploadFileCommand : IRequest<UploadResult>
{
    public IFormFile File { get; set; } = null!;
    public Guid CreatorId { get; set; }
}
