using Jarstat.Application.Commands;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;

namespace Jarstat.Application.Abstractions;

public abstract class AbstractDocumentCreationService : ICreationService<CreateDocumentCommand, Result<Document?>, Document?>
{
    public abstract Task<Result<Document?>> CreateAsync(CreateDocumentCommand request);
}
