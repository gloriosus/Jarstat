using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class UpdateUserCommand : IRequest<Result<User?>>
{
    public Guid Id { get; set; }
    public string Password { get; set; } = null!;
    public Guid LastUpdaterId { get; set; }
}
