using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace ReportingPortal.Services
{
    public class AuthHeaderHandler(ILocalStorageService localStorage) : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage = localStorage;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? token = await _localStorage.GetItemAsync<string>("token", CancellationToken.None);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}