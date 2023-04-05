using Jarstat.Domain.Entities;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Commands;

public class SearchUsersCommand : IRequest<Result<SearchValue<User>>>
{
    public string? UserName { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
