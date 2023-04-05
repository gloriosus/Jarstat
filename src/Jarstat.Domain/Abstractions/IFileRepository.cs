using Jarstat.Domain.Entities;

namespace Jarstat.Domain.Abstractions;

public interface IFileRepository
{
    Task<Entities.File> GetByIdAsync(Guid? id);
    Task<Entities.File> CreateAsync(Entities.File file);
    Entities.File Delete(Entities.File file);
}
