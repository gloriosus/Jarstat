using Jarstat.Application.Queries;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Handlers;

public class GetAllFoldersHandler : IRequestHandler<GetAllFoldersQuery, Result<Assortment<Folder>>>
{
    private readonly IFolderRepository _folderRepository;

    public GetAllFoldersHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<Result<Assortment<Folder>>> Handle(GetAllFoldersQuery request, CancellationToken cancellationToken)
    {
        var result = await _folderRepository.GetAllAsync();
        return result;
    }
}
