using Jarstat.Application.Commands;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;

namespace Jarstat.Application.Abstractions;

public abstract class AbstractDocumentUpdatingService : IUpdatingService<UpdateDocumentCommand, Result<Document?>, Document?>
{
    public abstract Task<Result<Document?>> UpdateAsync(UpdateDocumentCommand request);
}
