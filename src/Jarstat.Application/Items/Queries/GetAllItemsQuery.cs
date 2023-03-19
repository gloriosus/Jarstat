using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetAllItemsQuery : IRequest<Result<List<Item>>> { }
