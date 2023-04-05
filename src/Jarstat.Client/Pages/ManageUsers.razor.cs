using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Jarstat.Client.Enums;
using Jarstat.Client.Requests;
using Jarstat.Client.Responses;
using Jarstat.Client.Shared;
using Jarstat.Domain.Shared;
using AntDesign;

namespace Jarstat.Client.Pages;

public partial class ManageUsers
{
    [CascadingParameter]
    private Task<AuthenticationState> authenticationState { get; set; }

    [CascadingParameter]
    public ManageLayout Layout { get; set; }

    private ITable _table;
    private int _total;
    private int _currentPage = 1;
    private int _size = 10;

    private List<UserResponse> users = new();

    private bool _createUserVisible = false;
    private CreateUserRequest createUserRequest = new();

    private bool _updateUserVisible = false;
    private UpdateUserRequest updateUserRequest = new();

    private bool _deleteUserVisible = false;
    private Guid selectedUserId = Guid.Empty;

    private string searchKey = string.Empty;
    private SearchUsersRequest searchUsersRequest = new();

    private Guid LoggedInUserId { get; set; } = Guid.Empty;

    private async Task<Guid> GetLoggedInUserId()
    {
        var authState = await authenticationState;
        var claim = authState.User.FindFirst("UserId");
        if (claim is null)
            return default;

        return Guid.Parse(claim.Value);
    }

    protected override async Task OnInitializedAsync()
    {
        Layout.ActiveTab = Tab.Users;
        LoggedInUserId = await GetLoggedInUserId();
        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        searchUsersRequest.Skip = (_currentPage - 1) * _size;
        searchUsersRequest.Take = _size;

        var response = await Http.PostAsJsonAsync("api/users/search", searchUsersRequest);
        var result = await response.Content.ReadFromJsonAsync<Result<SearchResponse<UserResponse>>>();
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
        users = searchResult.Items;
    }

    private async Task OnCurrentPageChange()
    {
        await LoadUsers();
    }

    private async Task<bool> CreateUser()
    {
        var response = await Http.PostAsJsonAsync("api/users/create", createUserRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<UserResponse>>();

            Layout.ErrorType = AlertType.Error;
            Layout.ErrorCode = result!.Error.Code;
            Layout.ErrorMessage = result!.Error.Message;
            Layout.ShowError = true;

            return false;
        }

        return true;
    }

    private async Task CreateUserOnFinish()
    {
        if (string.IsNullOrWhiteSpace(createUserRequest.UserName))
            return;

        if (string.IsNullOrWhiteSpace(createUserRequest.Password))
            return;

        var authState = await authenticationState;
        var claim = authState.User.FindFirst("UserId");
        if (claim is null)
            return;

        var creatorId = Guid.Parse(claim.Value);
        createUserRequest.CreatorId = creatorId;

        bool isSuccess = await CreateUser();

        createUserRequest.UserName = null!;
        createUserRequest.Password = null!;

        _createUserVisible = false;

        if (!isSuccess) return;

        if (_currentPage != 1)
        {
            _currentPage = 1;
            return;
        }

        await LoadUsers();
    }

    private async Task<bool> UpdateUser()
    {
        var response = await Http.PutAsJsonAsync("api/users/update", updateUserRequest);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<UserResponse>>();

            Layout.ErrorType = AlertType.Error;
            Layout.ErrorCode = result!.Error.Code;
            Layout.ErrorMessage = result!.Error.Message;
            Layout.ShowError = true;

            return false;
        }

        return true;
    }

    private async Task UpdateUserOnFinish()
    {
        if (string.IsNullOrWhiteSpace(updateUserRequest.Password))
            return;

        var authState = await authenticationState;
        var claim = authState.User.FindFirst("UserId");
        if (claim is null)
            return;

        var lastUpdaterId = Guid.Parse(claim.Value);
        updateUserRequest.LastUpdaterId = lastUpdaterId;

        bool isSuccess = await UpdateUser();

        updateUserRequest.Password = null!;

        _updateUserVisible = false;

        if (isSuccess)
            await LoadUsers();
    }

    private async Task DeleteUser()
    {
        var response = await Http.DeleteAsync($"api/users/delete/{selectedUserId}");

        _deleteUserVisible = false;

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<UserResponse>>();

            Layout.ErrorType = AlertType.Error;
            Layout.ErrorCode = result!.Error.Code;
            Layout.ErrorMessage = result!.Error.Message;
            Layout.ShowError = true;

            return;
        }

        await LoadUsers();
    }
}
