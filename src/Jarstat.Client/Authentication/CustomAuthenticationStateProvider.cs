using Blazored.SessionStorage;
using Jarstat.Client.Extensions;
using Jarstat.Domain.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Jarstat.Client.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    private readonly ISessionStorageService _sessionStorage;

    public CustomAuthenticationStateProvider(ISessionStorageService sessionStorage) =>
        _sessionStorage = sessionStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserSession>("UserSession");
            if (userSession is null)
                return await Task.FromResult(new AuthenticationState(_anonymous));

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("UserId", userSession.UserId.ToString()),
                new Claim(ClaimTypes.Name, userSession.UserName)
            }, "JwtAuth"));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserSession? userSession)
    {
        ClaimsPrincipal claimsPrincipal;

        if (userSession is not null)
        {
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("UserId", userSession.UserId.ToString()),
                new Claim(ClaimTypes.Name, userSession.UserName)
            }));

            userSession.ExpiryTimeStamp = DateTime.Now.AddSeconds(userSession.ExpiresIn);
            await _sessionStorage.SaveItemEncryptedAsync("UserSession", userSession);
        }
        else
        {
            claimsPrincipal = _anonymous;
            await _sessionStorage.RemoveItemAsync("UserSession");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task<string> GetToken()
    {
        var result = string.Empty;

        try
        {
            var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserSession>("UserSession");
            if (userSession is not null && DateTime.Now < userSession.ExpiryTimeStamp)
                result = userSession.Token;
        }
        catch { }

        return result;
    }
}
