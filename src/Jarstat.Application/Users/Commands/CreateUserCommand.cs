using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class CreateUserCommand : IRequest<Result<User>>
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Guid? CreatorId { get; set; }
}
