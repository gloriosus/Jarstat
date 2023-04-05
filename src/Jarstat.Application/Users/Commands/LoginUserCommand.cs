using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class LoginUserCommand : IRequest<Result<UserSession>>
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
