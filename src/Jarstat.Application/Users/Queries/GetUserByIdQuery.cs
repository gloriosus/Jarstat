using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetUserByIdQuery : IRequest<Result<User?>>
{
    public GetUserByIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}
