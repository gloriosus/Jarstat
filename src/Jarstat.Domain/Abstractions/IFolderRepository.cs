using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;

namespace Jarstat.Domain.Abstractions;

public interface IFolderRepository
{
    Task<Assortment<Folder>> GetAllAsync();
    Task<Folder> GetByIdAsync(Guid id);
    Task<Folder> GetByVirtualPathAsync(string virtualPath);
    Task<Folder> CreateAsync(Folder folder);
    Folder Update(Folder folder);
    Folder Delete(Folder folder);
    Task<Assortment<Folder>> GetByParentId(Guid? parentId);
}
