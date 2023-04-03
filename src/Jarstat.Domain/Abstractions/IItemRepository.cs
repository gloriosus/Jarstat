using Jarstat.Domain.Records;

namespace Jarstat.Domain.Abstractions;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(Guid id);
    Task<List<Item>> GetRootsAsync();
    Task<List<Item>> GetChildrenAsync(Guid parentId);
}
