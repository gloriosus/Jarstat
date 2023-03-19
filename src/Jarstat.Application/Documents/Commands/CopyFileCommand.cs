using Jarstat.Domain.Entities;
using MediatR;

namespace Jarstat.Application.Commands;

public class CopyFileCommand : IRequest<Guid?>
{
    public string FileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public Guid CreatorId { get; set; }
}
