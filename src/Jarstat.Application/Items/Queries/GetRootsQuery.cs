using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Queries;

public class GetRootsQuery : IRequest<Result<Assortment<Item>>> { }
