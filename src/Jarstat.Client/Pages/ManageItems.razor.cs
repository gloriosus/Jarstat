﻿using Jarstat.Client.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Jarstat.Client.Enums;
using Jarstat.Client.Responses;
using Jarstat.Domain.Entities;
using System.Net.Http.Json;
using Jarstat.Domain.Shared;
using AntDesign;
using Microsoft.JSInterop;
using Jarstat.Client.Requests;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Components.Forms;
using Jarstat.Client.Extensions;
using Microsoft.Net.Http.Headers;

namespace Jarstat.Client.Pages;

public partial class ManageItems
{
    [CascadingParameter]
    private Task<AuthenticationState> authenticationState { get; set; }

    [CascadingParameter]
    public ManageLayout Layout { get; set; }

    private Assortment<ItemResponse> items { get; set; } = new();
    private Tree<ItemResponse> _tree = new();

    private string searchKey = string.Empty;
    private string[] expandedKeys = Array.Empty<string>();
    private ItemResponse? selectedItem = null;

    private bool CreateFolderButtonDisabled = true;
    private bool CreateDocumentButtonDisabled = true;
    private bool EditButtonDisabled = true;
    private bool DeleteButtonDisabled = true;
    private bool DownloadButtonDisabled = true;

    private bool _updateFolderVisible = false;
    private UpdateFolderRequest updateFolderRequest = new();

    private bool _createFolderVisible = false;
    private CreateFolderRequest createFolderRequest = new();

    private bool _updateDocumentVisible = false;
    private UpdateDocumentRequest updateDocumentRequest = new();

    private bool _createDocumentVisible = false;
    private CreateDocumentRequest createDocumentRequest = new();
    
    private bool _deleteItemVisible = false;

    private IBrowserFile uploadedFile;
    private FileUploadState fileUploadState = FileUploadState.None;
    private long MAX_FILE_SIZE = default;

    protected override async Task OnInitializedAsync()
    {
        Layout.ActiveTab = Tab.Items;

        var result = await Http.GetFromJsonAsync<Result<Assortment<ItemResponse>>>("api/items/roots");
        items = result?.Value!;

        MAX_FILE_SIZE = clientSettings.MaxFileUploadSizeInBytes;
    }

    private async Task<Guid?> GetLoggedUserId()
    {
        var authState = await authenticationState;
        var claim = authState.User.FindFirst("UserId");

        if (claim is null)
            return null;

        return Guid.Parse(claim.Value);
    }

    #region LoadTreeData
    private async Task ReloadChildren(ItemResponse itemResponse)
    {
        var result = await Http.GetFromJsonAsync<Result<Assortment<ItemResponse>>>($"api/items/children/{itemResponse.ItemId}");
        var children = result?.Value!;

        itemResponse.Children.Clear();
        foreach (var child in children)
            itemResponse.Children.Add(child);
    }

    private async Task OnNodeLoadDelayAsync(TreeEventArgs<ItemResponse> args)
    {
        await ReloadChildren(args.Node.DataItem);
    }
    #endregion

    private async Task MoveItemOnDrop(TreeEventArgs<ItemResponse> e)
    {
        var moveableItem = e.Node.DataItem;
        var targetItem = e.TargetNode.DataItem;
        var dropPosition = e.TargetNode.Expanded ? DropPosition.Inside : DropPosition.Below;

        var reorderItemRequest = new ReorderItemRequest
        {
            ItemId = moveableItem.ItemId,
            TargetItemId = targetItem.ItemId,
            DropPosition = dropPosition
        };

        var response = await Http.PostAsJsonAsync("api/items/reorder", reorderItemRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<FolderResponse>>();
            ShowError(result!.Error);

            return;
        }

        foreach (var key in expandedKeys)
        {
            var item = await items.FindItemResponseOrDefaultAsync(Guid.Parse(key));
            if (item is not null)
                await ReloadChildren(item);
        }
    }

    private async Task UpdateStateOnExpandChanged(TreeEventArgs<ItemResponse> e)
    {
        if (!e.Node.Expanded)
        {
            RemoveExpandedKeysRecursively(e.Node.DataItem);
            return;
        }

        expandedKeys = expandedKeys.Append(e.Node.DataItem.ItemId.ToString());
    }

    private async Task RemoveExpandedKeysRecursively(ItemResponse item)
    {
        foreach (var child in item.Children)
        {
            RemoveExpandedKeysRecursively(child);
        }

        expandedKeys = expandedKeys.Remove(item.ItemId.ToString());
    }

    private async Task SetButtonsOnTreeClick(TreeEventArgs<ItemResponse> e)
    {
        if (!e.Node.Selected)
        {
            CreateFolderButtonDisabled = true;
            CreateDocumentButtonDisabled = true;
            EditButtonDisabled = true;
            DeleteButtonDisabled = true;
            DownloadButtonDisabled = true;
            return;
        }

        if (e.Node.DataItem.Type.Equals("Document"))
        {
            CreateFolderButtonDisabled = true;
            CreateDocumentButtonDisabled = true;
            EditButtonDisabled = false;
            DeleteButtonDisabled = false;
            DownloadButtonDisabled = false;
            return;
        }

        if (e.Node.DataItem.Type.Equals("Folder"))
        {
            CreateFolderButtonDisabled = false;
            CreateDocumentButtonDisabled = false;
            EditButtonDisabled = false;
            DeleteButtonDisabled = false;
            DownloadButtonDisabled = true;
            return;
        }
    }

    #region DeleteItem
    private async Task DeleteItemOnFinish()
    {
        if (selectedItem is null)
            return;

        bool isSuccess = false;

        switch (selectedItem.Type)
        {
            case "Document":
                isSuccess = await DeleteDocument(selectedItem.ItemId);
                break;
            case "Folder":
                isSuccess = await DeleteFolder(selectedItem.ItemId);
                break;
        }

        _deleteItemVisible = false;

        if (isSuccess)
        {
            var parent = await items.FindItemResponseOrDefaultAsync((Guid)selectedItem.ParentId!);
            if (parent is null)
                return;

            await ReloadChildren(parent);
        }
    }

    private async Task<bool> DeleteDocument(Guid id)
    {
        var response = await Http.DeleteAsync($"api/documents/delete/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<DocumentResponse>>();
            ShowError(result!.Error);

            return false;
        }

        return true;
    }

    private async Task<bool> DeleteFolder(Guid id)
    {
        var response = await Http.DeleteAsync($"api/folders/delete/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<FolderResponse>>();
            ShowError(result!.Error);

            return false;
        }

        return true;
    }
    #endregion

    private async Task EditOnButtonClick()
    {
        if (selectedItem is null)
            return;

        switch (selectedItem.Type)
        {
            case "Document":
                await AssignUpdateDocumentRequestValues();
                break;
            case "Folder":
                await AssignUpdateFolderRequestValues();
                break;
        }
    }

    #region UpdateFolder
    private async Task<bool> UpdateFolder(UpdateFolderRequest updateFolderRequest)
    {
        var response = await Http.PutAsJsonAsync("api/folders/update", updateFolderRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<FolderResponse>>();
            ShowError(result!.Error);

            return false;
        }

        return true;
    }

    private async Task UpdateFolderOnFinish()
    {
        if (selectedItem is null)
            return;

        if (string.IsNullOrWhiteSpace(updateFolderRequest.DisplayName))
        {
            ShowError(new Error(
                "Error.ArgumentNullOrWhiteSpaceValue",
                "Значение поля 'Отображаемое имя' оказалось равным null, пустой строке или строке, состоящей только из пробелов"));

            _updateFolderVisible = false;

            return;
        }

        bool isSuccess = await UpdateFolder(updateFolderRequest);

        updateFolderRequest.FolderId = default;
        updateFolderRequest.DisplayName = string.Empty;
        updateFolderRequest.VirtualPath = string.Empty;
        updateFolderRequest.ParentId = null;
        updateFolderRequest.LastUpdaterId = default;

        _updateFolderVisible = false;

        if (isSuccess)
        {
            var parent = await items.FindItemResponseOrDefaultAsync((Guid)selectedItem.ParentId!);
            if (parent is null)
                return;

            await ReloadChildren(parent);
        }
    }

    private async Task AssignUpdateFolderRequestValues()
    {
        var userId = await GetLoggedUserId();
        if (userId is null) 
            return;

        updateFolderRequest.FolderId = selectedItem.ItemId;
        updateFolderRequest.DisplayName = selectedItem.DisplayName;
        updateFolderRequest.VirtualPath = "empty";
        updateFolderRequest.ParentId = selectedItem.ParentId;
        updateFolderRequest.LastUpdaterId = (Guid)userId;

        _updateFolderVisible = true;
    }
    #endregion

    #region UpdateDocument
    private async Task<bool> UpdateDocument(UpdateDocumentRequest updateDocumentRequest)
    {
        var response = await Http.PutAsJsonAsync("api/documents/update", updateDocumentRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<DocumentResponse>>();
            ShowError(result!.Error);

            return false;
        }

        return true;
    }

    private async Task UpdateDocumentOnFinish()
    {
        if (selectedItem is null)
            return;

        if (string.IsNullOrWhiteSpace(updateDocumentRequest.DisplayName))
        {
            ShowError(new Error(
                "Error.ArgumentNullOrWhiteSpaceValue",
                "Значение поля 'Отображаемое имя' оказалось равным null, пустой строке или строке, состоящей только из пробелов"));
            
            _updateDocumentVisible = false;

            return;
        }

        if (fileUploadState == FileUploadState.Success)
        {
            //updateDocumentRequest.FileId = uploadResult?.FileId;
        }

        var isSuccess = await UpdateDocument(updateDocumentRequest);

        updateDocumentRequest.DocumentId = default;
        updateDocumentRequest.DisplayName = string.Empty;
        updateDocumentRequest.FileName = string.Empty;
        updateDocumentRequest.FolderId = default;
        //updateDocumentRequest.Description = documentResponse.Value!.Description;
        updateDocumentRequest.LastUpdaterId = default;
        updateDocumentRequest.FileId = null;

        fileUploadState = FileUploadState.None;
        //uploadResult = UploadResult.Empty;

        _updateDocumentVisible = false;

        if (isSuccess)
        {
            var parent = await items.FindItemResponseOrDefaultAsync((Guid)selectedItem.ParentId!);
            if (parent is null)
                return;

            await ReloadChildren(parent);
        }
    }

    private async Task AssignUpdateDocumentRequestValues()
    {
        var userId = await GetLoggedUserId();
        if (userId is null)
            return;

        var documentResponse = await Http.GetFromJsonAsync<Result<DocumentResponse>>($"api/documents/{selectedItem.ItemId}");
        if (documentResponse is null || documentResponse.IsFailure)
            return;

        updateDocumentRequest.DocumentId = selectedItem.ItemId;
        updateDocumentRequest.DisplayName = selectedItem.DisplayName;
        updateDocumentRequest.FileName = documentResponse.Value!.FileName;
        updateDocumentRequest.FolderId = (Guid)selectedItem.ParentId!;
        //updateDocumentRequest.Description = documentResponse.Value!.Description;
        updateDocumentRequest.LastUpdaterId = (Guid)userId;
        updateDocumentRequest.FileId = documentResponse.Value!.FileId;

        _updateDocumentVisible = true;
    }
    #endregion

    #region CreateFolder
    private async Task CreateFolderOnButtonClick()
    {
        if (selectedItem is null)
            return;

        if (!selectedItem.Type.Equals("Folder"))
            return;

        var userId = await GetLoggedUserId();
        if (userId is null)
            return;

        _createFolderVisible = true;
    }

    private async Task<bool> CreateFolder(CreateFolderRequest createFolderRequest)
    {
        var response = await Http.PostAsJsonAsync("api/folders/create", createFolderRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<FolderResponse>>();
            ShowError(result!.Error);

            return false;
        }

        return true;
    }

    private async Task CreateFolderOnFinish()
    {
        if (selectedItem is null)
            return;

        if (string.IsNullOrWhiteSpace(createFolderRequest.DisplayName))
        {
            ShowError(new Error(
                "Error.ArgumentNullOrWhiteSpaceValue",
                "Значение поля 'Отображаемое имя' оказалось равным null, пустой строке или строке, состоящей только из пробелов"));

            _createFolderVisible = false;

            return;
        }

        createFolderRequest.VirtualPath = "empty";
        createFolderRequest.ParentId = selectedItem.ItemId;
        createFolderRequest.CreatorId = (Guid)await GetLoggedUserId();

        bool isSuccess = await CreateFolder(createFolderRequest);

        createFolderRequest.DisplayName = string.Empty;
        createFolderRequest.VirtualPath = string.Empty;
        createFolderRequest.ParentId = null;
        createFolderRequest.CreatorId = default;

        _createFolderVisible = false;

        if (isSuccess)
        {
            var parent = await items.FindItemResponseOrDefaultAsync(selectedItem.ItemId);
            if (parent is null)
                return;

            await ReloadChildren(parent);
        }
    }
    #endregion

    #region CreateDocument
    private async Task CreateDocumentOnButtonClick()
    {
        if (selectedItem is null)
            return;

        // Create document only when a folder is selected
        if (!selectedItem.Type.Equals("Folder"))
            return;

        var userId = await GetLoggedUserId();
        if (userId is null)
            return;

        _createDocumentVisible = true;
    }

    private async Task<bool> CreateDocument(CreateDocumentRequest createDocumentRequest)
    {
        var response = await Http.PostAsJsonAsync("api/documents/create", createDocumentRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<DocumentResponse>>();
            ShowError(result!.Error);

            return false;
        }

        return true;
    }

    private async Task CreateDocumentOnFinish()
    {
        if (selectedItem is null)
            return;

        if (string.IsNullOrWhiteSpace(createDocumentRequest.DisplayName))
        {
            ShowError(new Error(
                "Error.ArgumentNullOrWhiteSpaceValue", 
                "Значение поля 'Отображаемое имя' оказалось равным null, пустой строке или строке, состоящей только из пробелов"));
            _createDocumentVisible = false;
            return;
        }

        createDocumentRequest.FileId = await UploadFile();
        createDocumentRequest.FileName = uploadedFile.Name;
        createDocumentRequest.FolderId = selectedItem.ItemId;
        createDocumentRequest.CreatorId = (Guid)await GetLoggedUserId();

        bool isSuccess = await CreateDocument(createDocumentRequest);

        createDocumentRequest.Clear();
        fileUploadState = FileUploadState.None;
        _createDocumentVisible = false;

        if (isSuccess)
        {
            var parent = await items.FindItemResponseOrDefaultAsync(selectedItem.ItemId);
            if (parent is null)
                return;

            await ReloadChildren(parent);
        }
    }
    #endregion

    private void AssignFile(InputFileChangeEventArgs e)
    {
        if (e.File.Size == 0 || e.File.Size > MAX_FILE_SIZE)
        {
            fileUploadState = FileUploadState.Failure;
            return;
        }

        uploadedFile = e.File;
        fileUploadState = FileUploadState.Success;
    }

    private async Task DownloadFileOnDblClick(TreeEventArgs<ItemResponse> e)
    {
        var item = e.Node.DataItem;
        await js.InvokeVoidAsync(JSInteropConstants.TriggerFileDownload, null, $"api/documents/download/{item.ItemId}");
    }

    private void ShowError(Error error)
    {
        Layout.ErrorType = AlertType.Error;
        Layout.ErrorCode = error.Code;
        Layout.ErrorMessage = error.Message;
        Layout.ShowError = true;
    }

    private async Task<Guid?> UploadFile()
    {
        if (fileUploadState == FileUploadState.Success)
        {
            var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "api/documents/upload");
            var content = new MultipartFormDataContent
            {
                { new StreamContent(uploadedFile.OpenReadStream(MAX_FILE_SIZE)), "File", uploadedFile.Name },
                { new StringContent(((Guid)await GetLoggedUserId()).ToString()), "CreatorId" }
            };

            uploadRequest.Content = content;

            var response = await Http.SendAsync(uploadRequest);
            response.EnsureSuccessStatusCode();

            var uploadResult = await response.Content.ReadFromJsonAsync<Result<UploadValue>>();

            if (uploadResult is null)
                return null;

            if (uploadResult.IsFailure)
            {
                ShowError(uploadResult.Error);
                _createDocumentVisible = false;

                return null;
            }

            return uploadResult.Value!.FileId;
        }

        return null;
    }
}
