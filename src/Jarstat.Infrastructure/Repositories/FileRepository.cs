using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Jarstat.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Entities.File> GetByIdAsync(Guid? id)
    {
        var result = await _dbContext.Set<Domain.Entities.File>()
            .FirstOrDefaultAsync(f => f.Id == id);

        return result;
    }

    public async Task<Domain.Entities.File> CreateAsync(Domain.Entities.File file)
    {
        var result = await _dbContext.Set<Domain.Entities.File>().AddAsync(file);
        return result.Entity;
    }

    public Domain.Entities.File Delete(Domain.Entities.File file)
    {
        var result = _dbContext.Set<Domain.Entities.File>().Remove(file);
        return result.Entity;
    }
}
