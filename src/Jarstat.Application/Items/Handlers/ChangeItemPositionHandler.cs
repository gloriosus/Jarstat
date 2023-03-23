using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;
using System.Reflection.Metadata;
using System.Threading;

namespace Jarstat.Application.Handlers;

public class ChangeItemPositionHandler : IRequestHandler<ChangeItemPositionCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeItemPositionHandler(
        IDocumentRepository documentRepository, 
        IFolderRepository folderRepository,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _folderRepository = folderRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ChangeItemPositionCommand request, CancellationToken cancellationToken)
    {
        var items = await _itemRepository.GetAllAsync();
        var moveableItem = items.FirstOrDefault(i => i.Id == request.ItemId);
        var targetItem = items.FirstOrDefault(i => i.Id == request.TargetItemId);

        if (moveableItem is null || targetItem is null)
            return false;

        var targetItemParentId = GetParentId(targetItem, request.DropPosition);
        var itemsInFolder = GetFolderItems(targetItemParentId, items);
        var targetItemIsLastElement = IsTargetLast(targetItem, itemsInFolder, request.DropPosition);

        double sortOrder = targetItemIsLastElement ? long.MaxValue : GetSortOrder(targetItem, itemsInFolder, request.DropPosition);

        await ReorderItem(moveableItem, targetItemParentId, sortOrder);

        if (targetItemIsLastElement)
            await DowngradeItemsAbove(moveableItem, targetItemParentId);

        await _unitOfWork.SaveChangesAsync(default);

        return false;
    }

    private async Task DowngradeItemsAbove(Item item, Guid parentId)
    {
        var documents = await _documentRepository.GetAllAsync();
        var documentsInFolder = documents.Where(d => d.FolderId == parentId && d.Id != item.Id);
        foreach (var downgradeDocument in documentsInFolder)
        {
            downgradeDocument.ChangeSortOrder(downgradeDocument.SortOrder / 2);
            _documentRepository.Update(downgradeDocument);
        }

        var folders = await _folderRepository.GetAllAsync();
        var foldersInFolder = folders.Where(f => f.ParentId == parentId && f.Id != item.Id);
        foreach (var downgradeFolder in foldersInFolder)
        {
            downgradeFolder.ChangeSortOrder(downgradeFolder.SortOrder / 2);
            _folderRepository.Update(downgradeFolder);
        }
    }

    private async Task ReorderItem(Item item, Guid parentId, double sortOrder)
    {
        switch (item.Type)
        {
            case "Document":
                await ReorderDocument(item, parentId, sortOrder);
                break;
            case "Folder":
                await ReorderFolder(item, parentId, sortOrder);
                break;
            default:
                throw new ArgumentException("Неверное значение параметра", nameof(item.Type));
        }
    }

    private async Task ReorderDocument(Item item, Guid parentId, double sortOrder)
    {
        var document = await _documentRepository.GetByIdAsync(item.Id);

        if (item.ParentId == parentId)
        {
            var orderedDocument = document!.ChangeSortOrder(sortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        if (item.ParentId != parentId)
        {
            var targetFolder = await _folderRepository.GetByIdAsync(parentId);
            var movedDocument = document!.Update(document.DisplayName, document.FileName, targetFolder!, document.Description, document.LastUpdater, document.FileId);
            var orderedDocument = movedDocument.Value!.ChangeSortOrder(sortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }
    }

    private async Task ReorderFolder(Item item, Guid parentId, double sortOrder)
    {
        var folder = await _folderRepository.GetByIdAsync(item.Id);

        if (item.ParentId == parentId)
        {
            var orderedFolder = folder!.ChangeSortOrder(sortOrder);
            _folderRepository.Update(orderedFolder.Value!);
        }

        if (item.ParentId != parentId)
        {
            var targetFolder = await _folderRepository.GetByIdAsync(parentId);
            var movedFolder = folder!.Update(folder.DisplayName, folder.VirtualPath, targetFolder!, folder.LastUpdater);
            var orderedFolder = movedFolder.Value!.ChangeSortOrder(sortOrder);
            _folderRepository.Update(orderedFolder.Value!);
        }
    }

    private Guid GetParentId(Item targetItem, DropPosition dropPosition) =>
        dropPosition switch
        {
            DropPosition.Below => (Guid)targetItem.ParentId!,
            DropPosition.Inside => targetItem.Id,
            _ => throw new ArgumentException("Неверное значение параметра", nameof(dropPosition))
        };

    private IEnumerable<Item> GetFolderItems(Guid parentId, IList<Item> items) =>
        items.Where(i => i.ParentId == parentId)
             .OrderBy(i => i.SortOrder);

    private bool IsTargetLast(Item targetItem, IEnumerable<Item> folderItems, DropPosition dropPosition) =>
        dropPosition switch
        {
            DropPosition.Below => folderItems.SkipWhile(i => i.Id != targetItem.Id)
                                             .Count() > 1 ? false : true,
            DropPosition.Inside => folderItems.Any() ? false : true,
            _ => throw new ArgumentException("Неверное значение параметра", nameof(dropPosition))
        };

    private double GetSortOrder(Item targetItem, IEnumerable<Item> folderItems, DropPosition dropPosition) =>
        dropPosition switch
        {
            DropPosition.Below => folderItems.SkipWhile(i => i.Id != targetItem.Id)
                                             .Take(2)
                                             .Select(i => i.SortOrder / 2)
                                             .Sum(),
            DropPosition.Inside => folderItems.Take(1)
                                              .Select(i => i.SortOrder / 2)
                                              .Sum(),
            _ => throw new ArgumentException("Неверное значение параметра", nameof(dropPosition))
        };
}
