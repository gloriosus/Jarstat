using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetDocumentByIdQuery : IRequest<Result<Document>>
{
    public GetDocumentByIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}
