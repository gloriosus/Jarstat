using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class DownloadFileQuery : IRequest<Result<Document>>
{
    public DownloadFileQuery(Guid id) => Id = id;

    public Guid Id { get; }
}
