using Models.http;
using System.Net.Http.Json;

namespace Front.Services
{
    public class AuthService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync("api/Auth/register", registerRequest);
                RegisterResponse? content = await response.Content.ReadFromJsonAsync<RegisterResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new RegisterResponse
                {
                    Message = content?.Message ?? "An unknown error occurred.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync("api/Auth/login", new { email, password });
                LoginResponse? loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (response.IsSuccessStatusCode && loginResponse != null)
                {
                    return loginResponse;
                }

                return new LoginResponse
                {
                    Message = loginResponse?.Message ?? "Login failed.",
                    StatusCode = loginResponse?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Message = $"Unexpected error during login: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<LoginResponse> RefreshAsync()
        {
            try
            {
                HttpResponseMessage response = await _http.PostAsync("api/Auth/refresh", null);
                LoginResponse? refreshResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (response.IsSuccessStatusCode && refreshResponse != null)
                {
                    return refreshResponse;
                }
                return new LoginResponse
                {
                    Message = refreshResponse?.Message ?? "Refresh failed.",
                    StatusCode = refreshResponse?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Message = $"Unexpected error during refresh: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> LogoutAsync(string? deviceId = null)
        {
            try
            {
                HttpResponseMessage response = await _http.PostAsync($"api/Auth/logout/{deviceId}", null);
                Response? logoutResponse = await response.Content.ReadFromJsonAsync<Response>();
                if (response.IsSuccessStatusCode && logoutResponse != null)
                {
                    return logoutResponse;
                }
                return new Response
                {
                    Message = logoutResponse?.Message ?? "Logout failed.",
                    StatusCode = logoutResponse?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Message = $"Unexpected error during logout: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> LogoutAllAsync()
        {
            try
            {
                HttpResponseMessage response = await _http.PostAsync("api/Auth/logout/all", null);
                Response? logoutAllResponse = await response.Content.ReadFromJsonAsync<Response>();
                if (response.IsSuccessStatusCode && logoutAllResponse != null)
                {
                    return logoutAllResponse;
                }
                return new Response
                {
                    Message = logoutAllResponse?.Message ?? "Logout all failed.",
                    StatusCode = logoutAllResponse?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Message = $"Unexpected error during logout all: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}