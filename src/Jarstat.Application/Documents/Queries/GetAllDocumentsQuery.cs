using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetAllDocumentsQuery : IRequest<Result<List<Document>>> { }
