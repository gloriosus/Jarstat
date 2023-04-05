using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class CreateFolderCommand : IRequest<Result<Folder>>
{
    public string DisplayName { get; set; } = null!;
    public string VirtualPath { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Guid CreatorId { get; set; }
}
