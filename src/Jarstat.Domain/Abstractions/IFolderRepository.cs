using Jarstat.Domain.Entities;

namespace Jarstat.Domain.Abstractions;

public interface IFolderRepository
{
    Task<List<Folder>> GetAllAsync();
    Task<Folder?> GetByIdAsync(Guid id);
    Task<Folder?> GetByVirtualPathAsync(string virtualPath);
    Task<Folder?> CreateAsync(Folder folder);
    Folder? Update(Folder folder);
    Folder? Delete(Folder folder);
}
