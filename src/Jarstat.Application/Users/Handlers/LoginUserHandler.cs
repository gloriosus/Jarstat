using Jarstat.Application.Commands;
using Jarstat.Application.Services;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Jarstat.Application.Handlers;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<UserSession>>
{
    private readonly UserManager<User> _userManager;

    public LoginUserHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<UserSession>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var jwtAuthenticationService = new JwtAuthenticationService(_userManager);
        var userSession = await jwtAuthenticationService.GenerateJwtToken(request.UserName, request.Password);

        if (userSession is null)
            return DomainErrors.LoginFailed;

        return userSession;
    }
}
