using Jarstat.Application.Commands;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<User?>>
{
    private readonly UserManager<User> _userManager;

    public DeleteUserHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<User?>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null)
            return Result<User?>.Failure(DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString()));

        var userManagerResult = await _userManager.DeleteAsync(user);
        if (!userManagerResult.Succeeded)
            return Result<User?>.Failure(DomainErrors.Identity
                .WithParameters(userManagerResult.Errors.Select(x => x.Description).ToArray()));

        return user;
    }
}
