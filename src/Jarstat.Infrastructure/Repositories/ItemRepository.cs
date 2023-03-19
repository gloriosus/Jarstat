using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Records;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ItemRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<List<Item>> GetAllAsync()
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw("SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\"")
            .ToListAsync();

        return result;
    }

    public async Task<Item?> GetByIdAsync(Guid id)
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw($"SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"Id\" = '{id}'")
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<List<Item>> GetRootsAsync()
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw("SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"ParentId\" IS NULL ORDER BY \"SortOrder\"")
            .ToListAsync();

        return result;
    }

    public async Task<List<Item>> GetChildrenAsync(Guid parentId)
    {
        var result = await _dbContext.Set<Item>()
            .FromSqlRaw($"SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"ParentId\" = '{parentId}' ORDER BY \"SortOrder\"")
            .ToListAsync();

        return result;
    }

    public async Task<SearchResult<Item>> SearchDocuments(string? displayName, Guid[] parentIds, int skip = 0, int take = 10)
    {
        var result = _dbContext.Set<Item>()
            .FromSqlRaw($"SELECT \"Id\", \"DisplayName\", \"ParentId\", \"Type\", \"DateTimeCreated\", \"DateTimeUpdated\", \"SortOrder\" FROM \"ItemsView\" WHERE \"Type\" = 'Document'");

        if (!string.IsNullOrWhiteSpace(displayName))
            result = result.Where(i => i.DisplayName.ToLower().Contains(displayName.ToLower()));

        if (parentIds.Any())
            result = result.Where(i => parentIds.Contains((Guid)i.ParentId!));

        int count = await result.CountAsync();

        result = result.OrderByDescending(i => i.DateTimeCreated).Skip(skip).Take(take);

        List<Item> items = await result.ToListAsync();

        return new SearchResult<Item>(items, count);
    }
}
