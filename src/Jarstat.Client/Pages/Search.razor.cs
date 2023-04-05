using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Jarstat.Client.Responses;
using Jarstat.Domain.Shared;
using AntDesign;
using Jarstat.Client.Requests;
using Jarstat.Client.Shared;
using Jarstat.Client.Services;

namespace Jarstat.Client.Pages;

public partial class Search
{
    [CascadingParameter]
    public MainLayout Layout { get; set; }

    [Inject]
    private NotifyStateService? notifyStateService { get; set; }

    private int _total;
    private int _currentPage = 1;
    private int _size = 10;

    private SearchDocumentsRequest searchDocumentRequest = new();

    private IEnumerable<DocumentResponse> documents = new Assortment<DocumentResponse>();
    private IEnumerable<ItemResponse> folders = new Assortment<ItemResponse>();

    private string[] checkedKeys;

    private async Task LoadDocuments()
    {
        searchDocumentRequest.Skip = (_currentPage - 1) * _size;
        searchDocumentRequest.Take = _size;

        var response = await Http.PostAsJsonAsync("api/documents/search", searchDocumentRequest);
        var result = await response.Content.ReadFromJsonAsync<Result<SearchResponse<DocumentResponse>>>();
        if (!response.IsSuccessStatusCode)
        {
            Layout.ErrorType = AlertType.Error;
            Layout.ErrorCode = result!.Error.Code;
            Layout.ErrorMessage = result!.Error.Message;
            Layout.ShowError = true;

            return;
        }

        var searchResult = result?.Value!;
        _total = searchResult.Count;
        documents = searchResult.Items;
    }

    private async Task LoadChildren(TreeNode<ItemResponse> node)
    {
        var dataItem = node.DataItem;
        var collection = (Assortment<ItemResponse>)dataItem.Children;
        collection.Clear();

        var result = await Http.GetFromJsonAsync<Result<Assortment<ItemResponse>>>($"api/items/children/{dataItem.Id}");
        var children = result?.Value!.Where(i => i.Type.Equals("Folder"))!;

        foreach (var child in children)
            collection.Add(child);
    }

    private async Task OnNodeLoadDelayAsync(TreeEventArgs<ItemResponse> args)
    {
        await LoadChildren(args.Node);
    }

    private async Task OnCurrentPageChange()
    {
        await LoadDocuments();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadDocuments();

        var foldersResult = await Http.GetFromJsonAsync<Result<Assortment<ItemResponse>>>("api/items/roots");
        folders = foldersResult?.Value!;

        notifyStateService!.EventClick += OnSearchButtonClicked;
    }

    private async Task OnFolderCheck(TreeEventArgs<ItemResponse> e)
    {
        searchDocumentRequest.ParentIds = checkedKeys.Select(x => Guid.Parse(x)).ToArray();
        _currentPage = 1;

        await LoadDocuments();
    }

    private async void OnSearchButtonClicked(object? sender, EventArgs e)
    {
        searchDocumentRequest.DisplayName = Layout.SearchModel.DocumentName;
        _currentPage = 1;

        await LoadDocuments();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => notifyStateService!.EventClick -= OnSearchButtonClicked;
}
