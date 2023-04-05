using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetFolderByIdHandler : IRequestHandler<GetFolderByIdQuery, Result<Folder>>
{
    private readonly IFolderRepository _folderRepository;

    public GetFolderByIdHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<Result<Folder>> Handle(GetFolderByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _folderRepository.GetByIdAsync(request.Id);
        if (result is null)
            return DomainErrors.EntryNotFound
                .WithParameters(nameof(request.Id), typeof(Guid).ToString(), request.Id.ToString());

        return result;
    }
}
