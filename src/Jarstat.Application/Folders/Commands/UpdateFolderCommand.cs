using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class UpdateFolderCommand : IRequest<Result<Folder?>>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string VirtualPath { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Guid LastUpdaterId { get; set; }
}
