using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ReportingPortal.Services
{
    public class CustomAuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = await _localStorage.GetItemAsync<string>("token");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            ClaimsIdentity identity = new(ParseClaimsFromJwt(token), "jwt");
            ClaimsPrincipal user = new(identity);

            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication(string token)
        {
            ClaimsIdentity identity = new(ParseClaimsFromJwt(token), "jwt");
            ClaimsPrincipal user = new(identity);

            var something = Task.FromResult(new AuthenticationState(user));

            NotifyAuthenticationStateChanged(something);
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            string payload = jwt.Split('.')[1];
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(AddPadding(payload)));
            Dictionary<string, object>? claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (claims == null)
                return [];

            List<Claim> result = [];

            foreach (var kvp in claims)
            {
                if (kvp.Key == "role")
                {
                    if (kvp.Value == null)
                        continue;

                    result.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString() ?? ""));
                }
                else
                {
                    result.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                }
            }

            return result;
        }
        private static string AddPadding(string base64)
        {
            return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        }

    }

}