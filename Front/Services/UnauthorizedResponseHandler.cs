using Microsoft.AspNetCore.Components;
using Models.http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Front.Services;

public class UnauthorizedResponseHandler(
    NavigationManager navigation,
    CustomAuthStateProvider authProvider,
    IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    private readonly NavigationManager _navigation = navigation;
    private readonly CustomAuthStateProvider _authProvider = authProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static bool _isRefreshing = false;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var responseMapped = await base.SendAsync(request, cancellationToken);
        string responseBody = await responseMapped.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<Response>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (response.StatusCode == 401)
        {
            try
            {
                await _refreshLock.WaitAsync(cancellationToken);

                if (_isRefreshing)
                {
                    return await RetryOriginalRequestAsync(request, cancellationToken);
                }

                _isRefreshing = true;

                var refreshClient = _httpClientFactory.CreateClient("BaseClient");
                var refreshResponse = await refreshClient.PostAsync("api/Auth/refresh", null, cancellationToken);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    var loginResponse = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);

                    if (loginResponse is not null && loginResponse.StatusCode >= 200 && loginResponse.StatusCode < 300)
                    {
                        await _authProvider.ForceAuthenticationStateRefreshAsync();
                        return await RetryOriginalRequestAsync(request, cancellationToken);
                    }
                }

                return responseMapped;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHandler] Exception: {ex.Message}");
                _navigation.NavigateTo("/login");
                return responseMapped; // anche in caso di errore restituisci la risposta originale
            }
            finally
            {
                _isRefreshing = false;
                _refreshLock.Release();
            }
        }

        return responseMapped;
    }


    private async Task<HttpResponseMessage> RetryOriginalRequestAsync(HttpRequestMessage originalRequest, CancellationToken cancellationToken)
    {
        var retryRequest = CloneRequest(originalRequest);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
