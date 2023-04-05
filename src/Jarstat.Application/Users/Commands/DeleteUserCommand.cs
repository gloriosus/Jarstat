using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class DeleteUserCommand : IRequest<Result<User>>
{
    public DeleteUserCommand(Guid id) => Id = id;

    public Guid Id { get; set; }
}
