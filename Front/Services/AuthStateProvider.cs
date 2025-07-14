using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models.http;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Front.Services;

public class CustomAuthStateProvider(IHttpClientFactory factory, NavigationManager navigationManager) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private readonly NavigationManager _navigationManager = navigationManager;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        HttpClient _httpClient = factory.CreateClient("AuthorizedClient");
        try
        {
            var userResponse = await _httpClient.GetFromJsonAsync<UserResponse>("api/User/me");

            if (userResponse == null || userResponse.User == null)
                return new AuthenticationState(_anonymous);

            var user = userResponse.User;

            var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.Email),
                    new(ClaimTypes.Role, user.Role.ToString()),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

            var identity = new ClaimsIdentity(claims, "serverAuth");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);

        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    public async Task ForceAuthenticationStateRefreshAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

}
