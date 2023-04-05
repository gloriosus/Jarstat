using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using Jarstat.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ItemRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Assortment<Item>> GetAllAsync()
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw("SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\"")
            .ToAssortmentAsync();

        return result;
    }

    public async Task<Item> GetByIdAsync(Guid id)
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw($"SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"Id\" = '{id}'")
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<Assortment<Item>> GetRootsAsync()
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw("SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"ParentId\" IS NULL ORDER BY \"SortOrder\"")
            .ToAssortmentAsync();

        return result;
    }

    public async Task<Assortment<Item>> GetChildrenAsync(Guid parentId)
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw($"SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"ParentId\" = '{parentId}' ORDER BY \"SortOrder\"")
            .ToAssortmentAsync();

        return result;
    }
}
