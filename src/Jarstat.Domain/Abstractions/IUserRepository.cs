using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;

namespace Jarstat.Domain.Abstractions;

public interface IUserRepository
{
    Task<SearchValue<User>> SearchAsync(string? username, int skip = 0, int take = 10);
    Task<Assortment<User>> GetAllAsync();
}
