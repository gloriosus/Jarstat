using Jarstat.Application.Commands;
using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Entities;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore.Update;

namespace Jarstat.Application.Handlers;

public class ChangeItemPositionHandler : IRequestHandler<ChangeItemPositionCommand, Result<Item?>>
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

    public async Task<Result<Item?>> Handle(ChangeItemPositionCommand request, CancellationToken cancellationToken)
    {
        var items = await _itemRepository.GetAllAsync();
        var moveableItem = items.FirstOrDefault(i => i.Id == request.ItemId);
        var targetItem = items.FirstOrDefault(i => i.Id == request.TargetItemId);

        if (moveableItem is null)
            return Result<Item?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(moveableItem), typeof(Item).ToString()));

        if (targetItem is null)
            return Result<Item?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(targetItem), typeof(Item).ToString()));

        var targetItemParentId = GetParentId(targetItem, request.DropPosition);
        var itemsInFolder = GetFolderItems(targetItemParentId, items);
        var targetItemIsLastElement = IsTargetLast(targetItem, itemsInFolder, request.DropPosition);

        double sortOrder = targetItemIsLastElement ? long.MaxValue : GetSortOrder(targetItem, itemsInFolder, request.DropPosition);

        var reorderingResult = await ReorderItem(moveableItem, targetItemParentId, sortOrder);
        if (reorderingResult.IsFailure)
            return reorderingResult;

        var reorderedItem = reorderingResult.Value!;

        if (targetItemIsLastElement)
        {
            var downgradingResult = await DowngradeItemsAbove(reorderedItem, targetItemParentId);
            if (downgradingResult.IsFailure)
                return downgradingResult;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Item?>.Failure(DomainErrors.Exception
                .WithParameters(ex.InnerException?.Message!));
        }

        return reorderedItem;
    }

    private async Task<Result<Item?>> DowngradeItemsAbove(Item reorderedItem, Guid targetItemParentId)
    {
        var documentResult = await DowngradeDocumentsAbove(reorderedItem, targetItemParentId);
        if (documentResult.IsFailure)
            return Result<Item?>.Failure(documentResult.Error);

        var folderResult = await DowngradeFoldersAbove(reorderedItem, targetItemParentId);
        if (folderResult.IsFailure)
            return Result<Item?>.Failure(folderResult.Error);

        return Result<Item?>.Success(default);
    }

    private async Task<Result<Document?>> DowngradeDocumentsAbove(Item reorderedItem, Guid targetItemParentId)
    {
        var documentsInFolder = (await _documentRepository.GetByFolderId(targetItemParentId))
                                                          .Where(d => d.Id != reorderedItem.Id);
        foreach (var document in documentsInFolder)
        {
            var documentOrderingResult = document.ChangeSortOrder(document.SortOrder / 2);
            if (documentOrderingResult.IsFailure)
                return documentOrderingResult;

            var orderedDocument = documentOrderingResult.Value!;
            _documentRepository.Update(orderedDocument);
        }

        return Result<Document?>.Success(default);
    }

    private async Task<Result<Folder?>> DowngradeFoldersAbove(Item reorderedItem, Guid targetItemParentId)
    {
        var foldersInFolder = (await _folderRepository.GetByParentId(targetItemParentId))
                                                          .Where(d => d.Id != reorderedItem.Id);
        foreach (var folder in foldersInFolder)
        {
            var folderOrderingResult = folder.ChangeSortOrder(folder.SortOrder / 2);
            if (folderOrderingResult.IsFailure)
                return folderOrderingResult;

            var orderedFolder = folderOrderingResult.Value!;
            _folderRepository.Update(orderedFolder);
        }

        return Result<Folder?>.Success(default);
    }

    private async Task<Result<Item?>> ReorderItem(Item item, Guid targetItemParentId, double sortOrder)
    {
        switch (item.Type)
        {
            case "Document":
                var documentResult = await ReorderDocument(item, targetItemParentId, sortOrder);
                var document = documentResult.Value;
                return new Result<Item?>((Item?)document, documentResult.IsSuccess, documentResult.Error);
            case "Folder":
                var folderResult = await ReorderFolder(item, targetItemParentId, sortOrder);
                var folder = folderResult.Value;
                return new Result<Item?>((Item?)folder, folderResult.IsSuccess, folderResult.Error);
            default:
                throw new ArgumentException("Недопустимое значение параметра", nameof(item.Type));
        }
    }

    private async Task<Result<Document?>> ReorderDocument(Item documentItem, Guid targetItemParentId, double sortOrder)
    {
        var document = (await _documentRepository.GetByIdAsync(documentItem.Id))!;

        if (document.FolderId.Equals(targetItemParentId))
        {
            var documentOrderingResult = document.ChangeSortOrder(sortOrder);
            if (documentOrderingResult.IsFailure)
                return documentOrderingResult;

            var orderedDocument = documentOrderingResult.Value!;
            return _documentRepository.Update(orderedDocument);
        }
        else
        {
            var targetFolder = (await _folderRepository.GetByIdAsync(targetItemParentId))!;

            var documentMovingResult = document.Update(document.DisplayName, document.FileName, targetFolder, document.Description, document.LastUpdater, document.FileId);
            if (documentMovingResult.IsFailure)
                return documentMovingResult;

            var movedDocument = documentMovingResult.Value!;

            var documentOrderingResult = movedDocument.ChangeSortOrder(sortOrder);
            if (documentOrderingResult.IsFailure)
                return documentOrderingResult;

            var orderedDocument = documentOrderingResult.Value!;
            return _documentRepository.Update(orderedDocument);
        }
    }

    private async Task<Result<Folder?>> ReorderFolder(Item folderItem, Guid targetItemParentId, double sortOrder)
    {
        var folder = (await _folderRepository.GetByIdAsync(folderItem.Id))!;

        if (folder.ParentId.Equals(targetItemParentId))
        {
            var folderOrderingResult = folder.ChangeSortOrder(sortOrder);
            if (folderOrderingResult.IsFailure)
                return folderOrderingResult;

            var orderedFolder = folderOrderingResult.Value!;
            return _folderRepository.Update(orderedFolder);
        }
        else
        {
            var targetFolder = (await _folderRepository.GetByIdAsync(targetItemParentId))!;

            var folderMovingResult = folder.Update(folder.DisplayName, folder.VirtualPath, targetFolder, folder.LastUpdater);
            if (folderMovingResult.IsFailure)
                return folderMovingResult;

            var movedFolder = folderMovingResult.Value!;

            var folderOrderingResult = movedFolder.ChangeSortOrder(sortOrder);
            if (folderOrderingResult.IsFailure)
                return folderOrderingResult;

            var orderedFolder = folderOrderingResult.Value!;
            return _folderRepository.Update(orderedFolder);
        }
    }

    private Guid GetParentId(Item targetItem, DropPosition dropPosition) =>
        dropPosition switch
        {
            DropPosition.Below => (Guid)targetItem.ParentId!,
            DropPosition.Inside => targetItem.Id,
            _ => throw new ArgumentException("Недопустимое значение параметра", nameof(dropPosition))
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
            _ => throw new ArgumentException("Недопустимое значение параметра", nameof(dropPosition))
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
            _ => throw new ArgumentException("Недопустимое значение параметра", nameof(dropPosition))
        };
}
