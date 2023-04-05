using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class DeleteFolderCommand : IRequest<Result<Folder>>
{
    public DeleteFolderCommand(Guid id) => Id = id;

    public Guid Id { get; }
}
