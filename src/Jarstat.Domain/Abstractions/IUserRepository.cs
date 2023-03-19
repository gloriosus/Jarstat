using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;

namespace Jarstat.Domain.Abstractions;

public interface IUserRepository
{
    Task<SearchResult<User>> SearchAsync(string? username, int skip = 0, int take = 10);
}
