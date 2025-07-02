using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class AuthService(HttpClient http)
    {
        private readonly HttpClient _http = http;

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

    }
}