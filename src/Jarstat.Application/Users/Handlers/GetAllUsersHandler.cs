using Jarstat.Application.Queries;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Application.Handlers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<List<User>>>
{
    private readonly UserManager<User> _userManager;

    public GetAllUsersHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<List<User>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync();
        return users;
    }
}
