using Jarstat.Application.Commands;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<User>>
{
    private readonly UserManager<User> _userManager;

    public CreateUserHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var creator = await _userManager.FindByIdAsync(request.CreatorId?.ToString()!);
        if (request.CreatorId is not null && creator is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.CreatorId), typeof(Guid).ToString(), request.CreatorId?.ToString()!);

        var userCreationResult = User.Create(request.UserName, creator);
        if (userCreationResult.IsFailure)
            return userCreationResult!;

        var user = userCreationResult.Value!;

        var userManagerResult = await _userManager.CreateAsync(user, request.Password);
        if (!userManagerResult.Succeeded)
        {
            var errors = userManagerResult.Errors.Select(e => e.Description).ToArray();
            return DomainErrors.Identity.WithParameters(errors);
        }

        return userCreationResult;
    }
}
