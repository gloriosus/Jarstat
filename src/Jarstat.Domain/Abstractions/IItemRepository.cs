using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;

namespace Jarstat.Domain.Abstractions;

public interface IItemRepository
{
    Task<Assortment<Item>> GetAllAsync();
    Task<Item> GetByIdAsync(Guid id);
    Task<Assortment<Item>> GetRootsAsync();
    Task<Assortment<Item>> GetChildrenAsync(Guid parentId);
}
