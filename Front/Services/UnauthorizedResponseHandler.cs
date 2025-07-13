using Front.Services;
using Microsoft.AspNetCore.Components;
using Models.http;
using System.Net.Http.Json;
using System.Text.Json;

public class UnauthorizedResponseHandler : DelegatingHandler
{
    private readonly NavigationManager _navigation;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public UnauthorizedResponseHandler(
        NavigationManager navigation,
        CustomAuthStateProvider authProvider,
        IHttpClientFactory httpClientFactory)
    {
        _navigation = navigation;
        _authProvider = authProvider;
        _httpClientFactory = httpClientFactory;
    }

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

                if (!_isRefreshing)
                {
                    _isRefreshing = true;
                    var client = _httpClientFactory.CreateClient("AuthorizedClient");

                    HttpResponseMessage responseRefresh = await client.PostAsync("api/Auth/refresh", null, cancellationToken);
                    var refreshResponse = await responseRefresh.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);

                    if (refreshResponse.StatusCode >= 200 && refreshResponse.StatusCode < 300 && refreshResponse != null)
                    {
                        await _authProvider.ForceAuthenticationStateRefreshAsync();
                        _isRefreshing = false;

                        return await base.SendAsync(CloneRequest(request), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during unauthorized response handling: {ex.Message}");
                _isRefreshing = false;
                _refreshLock.Release();
                _navigation.NavigateTo("/login");
            }
            finally
            {
                _isRefreshing = false;
                _refreshLock.Release();
            }
        }

        return responseMapped;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
