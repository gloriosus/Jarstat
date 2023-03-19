using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class DeleteDocumentCommand : IRequest<Result<Document?>>
{
    public DeleteDocumentCommand(Guid id) => Id = id;

    public Guid Id { get; set; }
}
