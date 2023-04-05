using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetFolderByIdQuery : IRequest<Result<Folder>>
{
    public GetFolderByIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}
