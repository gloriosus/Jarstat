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
        var item = await _itemRepository.GetByIdAsync(request.ItemId);

        if (item is null)
            return false;

        switch (item.Type)
        {
            case "Document":
                return await ChangeDocumentSortOrder(item, request.TargetItemId);
            case "Folder":
                return await ChangeFolderSortOrder(item, request.TargetItemId);
        }

        return false;
    }

    private async Task<bool> ChangeDocumentSortOrder(Item item, Guid targetItemId)
    {
        var targetItem = await _itemRepository.GetByIdAsync(targetItemId);

        if (targetItem is null)
            return false;

        switch (targetItem.Type)
        {
            case "Document":
                return await ChangeDocumentSortOrderWhenDocumentTarget(item, targetItem);
            case "Folder":
                return await ChangeDocumentSortOrderWhenFolderTarget(item, targetItem);
        }

        return false;
    }

    private async Task<bool> DocumentToDocument(Item item, Item targetItem)
    {
        var items = await _itemRepository.GetAllAsync();
        var newSortOrder = items
            .Where(i => i.ParentId == targetItem.ParentId)
            .OrderBy(i => i.SortOrder)
            .SkipWhile(i => i.Id != targetItem.Id)
            .Take(2)
            .Select(i => i.SortOrder / 2)
            .Sum();

        var document = await _documentRepository.GetByIdAsync(item.Id);

        if (item.ParentId == targetItem.ParentId)
        {
            var orderedDocument = document!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        if (item.ParentId != targetItem.ParentId)
        {
            var targetFolder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
            var movedDocument = document!.Update(document.DisplayName, document.FileName, targetFolder!, document.Description, document.LastUpdater, document.FileId);
            var orderedDocument = movedDocument.Value!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        await _unitOfWork.SaveChangesAsync(default);

        return true;
    }

    private async Task<bool> DocumentToDocumentLast(Item item, Item targetItem)
    {
        var newSortOrder = long.MaxValue;
        var document = await _documentRepository.GetByIdAsync(item.Id);

        if (item.ParentId == targetItem.ParentId)
        {
            var orderedDocument = document!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        if (item.ParentId != targetItem.ParentId)
        {
            var targetFolder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
            var movedDocument = document!.Update(document.DisplayName, document.FileName, targetFolder!, document.Description, document.LastUpdater, document.FileId);
            var orderedDocument = movedDocument.Value!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        var documents = await _documentRepository.GetAllAsync();
        var documentsInFolder = documents.Where(d => d.FolderId == targetItem.ParentId && d.Id != item.Id);
        foreach (var downgradeDocument in documentsInFolder)
        {
            downgradeDocument.ChangeSortOrder(downgradeDocument.SortOrder / 2);
            _documentRepository.Update(downgradeDocument);
        }

        var folders = await _folderRepository.GetAllAsync();
        var foldersInFolder = folders.Where(f => f.ParentId == targetItem.ParentId && f.Id != item.Id);
        foreach (var downgradeFolder in foldersInFolder)
        {
            downgradeFolder.ChangeSortOrder(downgradeFolder.SortOrder / 2);
            _folderRepository.Update(downgradeFolder);
        }

        await _unitOfWork.SaveChangesAsync(default);

        return true;
    }

    private async Task<bool> DocumentToFolder(Item item, Item targetItem)
    {
        var items = await _itemRepository.GetAllAsync();
        var newSortOrder = items
            .Where(i => i.ParentId == targetItem.ParentId)
            .OrderBy(i => i.SortOrder)
            .SkipWhile(i => i.Id != targetItem.Id)
            .Take(2)
            .Select(i => i.SortOrder / 2)
            .Sum();

        var document = await _documentRepository.GetByIdAsync(item.Id);

        if (item.ParentId == targetItem.ParentId)
        {
            var orderedDocument = document!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        if (item.ParentId != targetItem.ParentId)
        {
            var targetFolder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
            var movedDocument = document!.Update(document.DisplayName, document.FileName, targetFolder!, document.Description, document.LastUpdater, document.FileId);
            var orderedDocument = movedDocument.Value!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        await _unitOfWork.SaveChangesAsync(default);

        return true;
    }

    private async Task<bool> DocumentToFolderExpanded(Item item, Item targetItem)
    {
        var items = await _itemRepository.GetAllAsync();
        var newSortOrder = items
            .Where(i => i.ParentId == targetItem.Id)
            .OrderBy(i => i.SortOrder)
            .Take(1)
            .Select(i => i.SortOrder / 2)
            .Sum();

        var document = await _documentRepository.GetByIdAsync(item.Id);

        if (item.ParentId == targetItem.Id)
        {
            var orderedDocument = document!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        if (item.ParentId != targetItem.Id)
        {
            var targetFolder = await _folderRepository.GetByIdAsync(targetItem.Id);
            var movedDocument = document!.Update(document.DisplayName, document.FileName, targetFolder!, document.Description, document.LastUpdater, document.FileId);
            var orderedDocument = movedDocument.Value!.ChangeSortOrder(newSortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        await _unitOfWork.SaveChangesAsync(default);

        return true;
    }

    private async Task<bool> DocumentToFolderLast();

    private async Task<bool> ReorderDocument(Item item, Item targetItem, DropPosition dropPosition)
    {
        var items = await _itemRepository.GetAllAsync();

        bool isTargetLastItem = true;

        switch (dropPosition)
        {
            case DropPosition.Below:
                isTargetLastItem = items.Where(i => i.ParentId == targetItem.ParentId)
                                        .OrderBy(i => i.SortOrder)
                                        .SkipWhile(i => i.Id != targetItem.Id)
                                        .Count() > 1 ? false : true;
                break;
            case DropPosition.Inside:
                isTargetLastItem = items.Where(i => i.ParentId == targetItem.Id)
                                        .OrderBy(i => i.SortOrder)
                                        .SkipWhile(i => i.Id != targetItem.Id)
                                        .Count() > 1 ? false : true;
                break;
        }

        double sortOrder = long.MaxValue;

        if (!isTargetLastItem)
        {
            switch (dropPosition)
            {
                case DropPosition.Below:
                    sortOrder = items.Where(i => i.ParentId == targetItem.ParentId)
                                     .OrderBy(i => i.SortOrder)
                                     .SkipWhile(i => i.Id != targetItem.Id)
                                     .Take(2)
                                     .Select(i => i.SortOrder / 2)
                                     .Sum();
                    break;
                case DropPosition.Inside:
                    sortOrder = items.Where(i => i.ParentId == targetItem.Id)
                                     .OrderBy(i => i.SortOrder)
                                     .Take(1)
                                     .Select(i => i.SortOrder / 2)
                                     .Sum();
                    break;
            }
        }

        var document = await _documentRepository.GetByIdAsync(item.Id);

        if (item.ParentId == targetItem.ParentId)
        {
            var orderedDocument = document!.ChangeSortOrder(sortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }

        if (item.ParentId != targetItem.ParentId)
        {
            var targetFolder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
            var movedDocument = document!.Update(document.DisplayName, document.FileName, targetFolder!, document.Description, document.LastUpdater, document.FileId);
            var orderedDocument = movedDocument.Value!.ChangeSortOrder(sortOrder);
            _documentRepository.Update(orderedDocument.Value!);
        }
    }

    //private async Task<bool> ChangeDocumentSortOrderWhenDocumentTarget(Item item, Item targetItem)
    //{
    //    var items = await _itemRepository.GetAllAsync();
    //    var newSortOrder = items
    //        .Where(i => i.ParentId == targetItem.ParentId)
    //        .OrderBy(i => i.SortOrder)
    //        .SkipWhile(i => i.Id != targetItem.Id)
    //        .Take(2)
    //        .Select(i => i.SortOrder / 2)
    //        .Sum();

    //    var document = await _documentRepository.GetByIdAsync(item.Id);

    //    if (item.ParentId == targetItem.ParentId)
    //    {
    //        var updatedDocument = document.ChangeSortOrder(newSortOrder);
    //        var result = _documentRepository.Update(updatedDocument.Value!);
    //    }

    //    if (item.ParentId != targetItem.ParentId)
    //    {
    //        var folder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
    //        var updatedDocument = document.Update(document.DisplayName, document.FileName, folder!, document.Description, document.LastUpdater, document.FileId);
    //        var orderedDocument = updatedDocument.Value!.ChangeSortOrder(newSortOrder);
    //        var result = _documentRepository.Update(orderedDocument.Value!);
    //    }

    //    await _unitOfWork.SaveChangesAsync(default);

    //    return true;
    //}

    //private async Task<bool> ChangeDocumentSortOrderWhenFolderTarget(Item item, Item targetItem)
    //{
    //    var items = await _itemRepository.GetAllAsync();
    //    var newSortOrder = items
    //        .Where(i => i.ParentId == targetItem.ParentId)
    //        .OrderBy(i => i.SortOrder)
    //        .SkipWhile(i => i.Id != targetItem.Id)
    //        .Take(2)
    //        .Select(i => i.SortOrder / 2)
    //        .Sum();

    //    var document = await _documentRepository.GetByIdAsync(item.Id);

    //    if (item.ParentId == targetItem.ParentId)
    //    {
    //        var updatedDocument = document.ChangeSortOrder(newSortOrder);
    //        var result = _documentRepository.Update(updatedDocument.Value!);
    //    }

    //    if (item.ParentId != targetItem.ParentId)
    //    {
    //        var folder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
    //        var updatedDocument = document.Update(document.DisplayName, document.FileName, folder!, document.Description, document.LastUpdater, document.FileId);
    //        var orderedDocument = updatedDocument.Value!.ChangeSortOrder(newSortOrder);
    //        var result = _documentRepository.Update(orderedDocument.Value!);
    //    }

    //    await _unitOfWork.SaveChangesAsync(default);

    //    return true;
    //}

    //private async Task<bool> ChangeFolderSortOrder(Item item, Guid targetItemId)
    //{
    //    var targetItem = await _itemRepository.GetByIdAsync(targetItemId);

    //    if (targetItem is null)
    //        return false;

    //    switch (targetItem.Type)
    //    {
    //        case "Document":
    //            return await ChangeFolderSortOrderWhenDocumentTarget(item, targetItem);
    //        case "Folder":
    //            return await ChangeFolderSortOrderWhenFolderTarget(item, targetItem);
    //    }

    //    return false;
    //}

    //private async Task<bool> ChangeFolderSortOrderWhenDocumentTarget(Item item, Item targetItem)
    //{
    //    var items = await _itemRepository.GetAllAsync();
    //    var newSortOrder = items
    //        .Where(i => i.ParentId == targetItem.ParentId)
    //        .OrderBy(i => i.SortOrder)
    //        .SkipWhile(i => i.Id != targetItem.Id)
    //        .Take(2)
    //        .Select(i => i.SortOrder / 2)
    //        .Sum();

    //    var folder = await _folderRepository.GetByIdAsync(item.Id);

    //    if (item.ParentId == targetItem.ParentId)
    //    {
    //        var updatedFolder = folder.ChangeSortOrder(newSortOrder);
    //        var result = _folderRepository.Update(updatedFolder.Value!);
    //    }

    //    if (item.ParentId != targetItem.ParentId)
    //    {
    //        var parentFolder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
    //        var updatedFolder = folder.Update(folder.DisplayName, folder.VirtualPath, parentFolder!, folder.LastUpdater);
    //        var orderedFolder = updatedFolder.Value!.ChangeSortOrder(newSortOrder);
    //        var result = _folderRepository.Update(orderedFolder.Value!);
    //    }

    //    await _unitOfWork.SaveChangesAsync(default);

    //    return true;
    //}

    //private async Task<bool> ChangeFolderSortOrderWhenFolderTarget(Item item, Item targetItem)
    //{
    //    if (item.Id == targetItem.Id)
    //        return false;

    //    var items = await _itemRepository.GetAllAsync();
    //    var newSortOrder = items
    //        .Where(i => i.ParentId == targetItem.ParentId)
    //        .OrderBy(i => i.SortOrder)
    //        .SkipWhile(i => i.Id != targetItem.Id)
    //        .Take(2)
    //        .Select(i => i.SortOrder / 2)
    //        .Sum();

    //    var folder = await _folderRepository.GetByIdAsync(item.Id);

    //    if (item.ParentId == targetItem.ParentId)
    //    {
    //        var updatedFolder = folder.ChangeSortOrder(newSortOrder);
    //        var result = _folderRepository.Update(updatedFolder.Value!);
    //    }

    //    if (item.ParentId != targetItem.ParentId)
    //    {
    //        var parentFolder = await _folderRepository.GetByIdAsync((Guid)targetItem.ParentId!);
    //        var updatedFolder = folder.Update(folder.DisplayName, folder.VirtualPath, parentFolder!, folder.LastUpdater);
    //        var orderedFolder = updatedFolder.Value!.ChangeSortOrder(newSortOrder);
    //        var result = _folderRepository.Update(orderedFolder.Value!);
    //    }

    //    await _unitOfWork.SaveChangesAsync(default);

    //    return true;
    //}
}
