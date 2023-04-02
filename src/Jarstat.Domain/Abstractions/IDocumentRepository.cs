using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;

namespace Jarstat.Domain.Abstractions;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(Guid id);
    Task<Document?> CreateAsync(Document document);
    Document? Delete(Document document);
    Document? Update(Document document);
    Task<List<Document>> GetByFolderId(Guid folderId);
    Task<SearchResult<Document>> SearchDocuments(string? displayName, Guid[] parentIds, int skip = 0, int take = 10);
}
