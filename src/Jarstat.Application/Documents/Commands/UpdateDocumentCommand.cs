using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class UpdateDocumentCommand : IRequest<Result<Document>>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid FolderId { get; set; }
    public string? Description { get; set; }
    public Guid LastUpdaterId { get; set; }
    public Guid? FileId { get; set; }
}
