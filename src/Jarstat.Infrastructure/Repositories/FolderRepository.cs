using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FolderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Folder>> GetAllAsync()
    {
        var result = await _dbContext.Set<Folder>()
            .Include(f => f.Creator)
            .Include(f => f.LastUpdater)
            .ToListAsync();

        return result;
    }

    public async Task<Folder?> GetByIdAsync(Guid id)
    {
        var result = await _dbContext.Set<Folder>()
            .Include(f => f.Creator)
            .Include(f => f.LastUpdater)
            .FirstOrDefaultAsync(f => f.Id == id);

        return result;
    }

    public async Task<Folder?> GetByVirtualPathAsync(string virtualPath)
    {
        var result = await _dbContext.Set<Folder>()
            .FirstOrDefaultAsync(f => f.VirtualPath.ToLower().Equals(virtualPath.ToLower()));

        return result;
    }

    public async Task<Folder?> CreateAsync(Folder folder)
    {
        var result = await _dbContext.Set<Folder>().AddAsync(folder);
        return result.Entity;
    }

    public Folder? Update(Folder folder)
    {
        var result = _dbContext.Set<Folder>().Update(folder);
        return result.Entity;
    }

    public Folder? Delete(Folder folder)
    {
        var result = _dbContext.Set<Folder>().Remove(folder);
        return result.Entity;
    }

    public async Task<List<Folder>> GetByParentId(Guid? parentId)
    {
        var result = await _dbContext.Set<Folder>()
            .Include(f => f.Creator)
            .Include(f => f.LastUpdater)
            .Where(f => f.ParentId.Equals(parentId))
            .ToListAsync();

        return result;
    }
}
