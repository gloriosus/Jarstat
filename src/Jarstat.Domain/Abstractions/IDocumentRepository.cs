using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;

namespace Jarstat.Domain.Abstractions;

public interface IDocumentRepository
{
    Task<Assortment<Document>> GetAllAsync();
    Task<Document> GetByIdAsync(Guid id);
    Task<Document> CreateAsync(Document document);
    Document Delete(Document document);
    Document Update(Document document);
    Task<Assortment<Document>> GetByFolderId(Guid folderId);
    Task<SearchValue<Document>> SearchDocuments(string? displayName, Guid[] parentIds, int skip = 0, int take = 10);
}
