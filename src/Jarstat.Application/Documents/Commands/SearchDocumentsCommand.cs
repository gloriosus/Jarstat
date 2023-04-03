using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class SearchDocumentsCommand : IRequest<Result<SearchResult<Document>>>
{
    public string? DisplayName { get; set; }
    public Guid[] ParentIds { get; set; } = Array.Empty<Guid>();
    public int Skip { get; set; }
    public int Take { get; set; }
}
