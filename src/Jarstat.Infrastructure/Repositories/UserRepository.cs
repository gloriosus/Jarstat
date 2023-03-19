using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<SearchResult<User>> SearchAsync(string? username, int skip = 0, int take = 10)
    {
        var result = _dbContext.Set<User>()
            .FromSqlRaw($"SELECT * FROM \"AspNetUsers\" WHERE \"UserName\" != 'root'");

        if (!string.IsNullOrWhiteSpace(username))
            result = result.Where(i => i.UserName!.ToLower().Contains(username.ToLower()));

        int count = await result.CountAsync();

        result = result.OrderByDescending(i => i.DateTimeCreated).Skip(skip).Take(take);

        List<User> users = await result.ToListAsync();

        return new SearchResult<User>(users, count);
    }
}
