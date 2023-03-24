using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Jarstat.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DocumentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Document>> GetAllAsync()
    {
        var result = await _dbContext.Set<Document>()
            .Include(document => document.Folder)
            .Include(document => document.Creator)
            .Include(document => document.LastUpdater)
            .ToListAsync();

        return result;
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        var result = await _dbContext.Set<Document>()
            .Include(document => document.Folder)
            .Include(document => document.Creator)
            .Include(document => document.LastUpdater)
            .FirstOrDefaultAsync(document => document.Id == id);

        return result;
    }

    public async Task<Document?> CreateAsync(Document document)
    {
        var result = await _dbContext.Set<Document>().AddAsync(document);
        return result.Entity;
    }

    public Document? Delete(Document document)
    {
        var result = _dbContext.Set<Document>().Remove(document);
        return result.Entity;
    }

    public Document? Update(Document document)
    {
        var result = _dbContext.Set<Document>().Update(document);
        return result.Entity;
    }

    public async Task<List<Document>> GetByFolderId(Guid folderId)
    {
        var result = await _dbContext.Set<Document>()
            .Include(document => document.Folder)
            .Include(document => document.Creator)
            .Include(document => document.LastUpdater)
            .Where(document => document.FolderId.Equals(folderId))
            .ToListAsync();

        return result;
    }
}
