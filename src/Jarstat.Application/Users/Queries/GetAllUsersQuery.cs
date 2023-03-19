using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetAllUsersQuery : IRequest<Result<List<User>>>
{
}
