using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using Jarstat.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public UserRepository(
        ApplicationDbContext dbContext,
        UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<SearchValue<User>> SearchAsync(string? username, int skip = 0, int take = 10)
    {
        var result = _dbContext.Set<User>()
            .FromSqlRaw($"SELECT * FROM \"AspNetUsers\" WHERE \"UserName\" != 'root'");

        if (!string.IsNullOrWhiteSpace(username))
            result = result.Where(i => i.UserName!.ToLower().Contains(username.ToLower()));

        int count = await result.CountAsync();

        result = result.OrderByDescending(i => i.DateTimeCreated).Skip(skip).Take(take);

        List<User> users = await result.ToListAsync();

        return new SearchValue<User>(users, count);
    }

    public async Task<Assortment<User>> GetAllAsync()
    {
        var users = await _userManager.Users.ToAssortmentAsync();
        return users;
    }
}
