using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class SearchUsersHandler : IRequestHandler<SearchUsersCommand, Result<SearchValue<User>>>
{
    private readonly IUserRepository _userRepository;

    public SearchUsersHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<Result<SearchValue<User>>> Handle(SearchUsersCommand request, CancellationToken cancellationToken)
    {
        if (request.Skip < 0)
            return DomainErrors.ArgumentNotAcceptableValue
                .WithParameters(nameof(request.Skip), typeof(int).ToString(), request.Skip.ToString());

        if (request.Take <= 0)
            return DomainErrors.ArgumentNotAcceptableValue
                .WithParameters(nameof(request.Take), typeof(int).ToString(), request.Take.ToString());

        var result = await _userRepository.SearchAsync(request.UserName, request.Skip, request.Take);

        return result;
    }
}
