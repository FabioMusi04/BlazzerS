using Models.front;
using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class UserService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<UsersPaginatedResponse> GetAllAsync(UsersPaginatedRequest request)
        {
            try
            {
                List<string> queryParams =
                [
                        $"page={request.Page}",
                        $"pageSize={request.PageSize}"
                ];

                if (request.Role.HasValue)
                {
                    queryParams.Add($"role={request.Role}");
                }

                if (request.EmailConfirmed.HasValue)
                {
                    queryParams.Add($"emailConfirmed={request.EmailConfirmed.Value.ToString().ToLower()}");
                }

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");
                }

                if (!string.IsNullOrWhiteSpace(request.SortField))
                {
                    queryParams.Add($"sortField={Uri.EscapeDataString(request.SortField)}");
                }

                if (request.SortAscending.HasValue)
                {
                    queryParams.Add($"sortAscending={request.SortAscending.Value.ToString().ToLower()}");
                }

                string queryString = string.Join("&", queryParams);
                HttpResponseMessage response = await _http.GetAsync($"api/User?{queryString}");

                UsersPaginatedResponse? content = await response.Content.ReadFromJsonAsync<UsersPaginatedResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new UsersPaginatedResponse
                {
                    Message = content?.Message ?? "An unknown error occurred.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Items = []
                };
            }
            catch (Exception ex)
            {
                return new UsersPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Items = []
                };
            }
        }

        public async Task<UserResponse> GetMeAsync()
        {
            try
            {
                HttpResponseMessage response = await _http.GetAsync("api/User/me");
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new UserResponse
                {
                    Message = content?.Message ?? "An unknown error occurred.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<UserResponse> UpdateMeAsync(UserPutModel updatedUser)
        {
            try
            {
                HttpResponseMessage response = await _http.PutAsJsonAsync("api/User/me", updatedUser);
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new UserResponse
                {
                    Message = content?.Message ?? "Error updating user",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> UpdateMePasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                ChangePasswordFormModel payload = new()
                {
                    CurrentPassword = oldPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                };

                HttpResponseMessage response = await _http.PutAsJsonAsync("api/User/me/password", payload);
                Response? content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                    return content;

                return new Response
                {
                    Message = content?.Message ?? "Error updating password",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> DeleteMeAsync()
        {
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync("api/User/me");
                Response? content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                    return content;

                return new Response
                {
                    Message = content?.Message ?? "Error deleting account",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> DeleteAsync(int userId)
        {
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync($"api/User/{userId}");
                Response? content = await response.Content.ReadFromJsonAsync<Response>();
                if (response.IsSuccessStatusCode && content != null)
                    return content;

                return new Response
                {
                    Message = content?.Message ?? "Error deleting user",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<UserResponse> CreateResetPasswordAsync(string email)
        {
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync("api/User/reset-password", new ResetPasswordRequest() { Email = email });
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();
                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }
                return new UserResponse
                {
                    Message = content?.Message ?? "Error resetting password",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<UserResponse> VerifyResetPasswordTokenAsync(string token)
        {
            try
            {
                HttpResponseMessage response = await _http.GetAsync($"api/User/reset-password/{Uri.EscapeDataString(token)}");
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();
                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }
                return new UserResponse
                {
                    Message = content?.Message ?? "Error verifying reset password token",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<UserResponse> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                ResetPasswordFormModel payload = new()
                {
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                };
                HttpResponseMessage response = await _http.PostAsJsonAsync($"api/User/reset-password/{Uri.EscapeDataString(token)}", payload);
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();
                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }
                return new UserResponse
                {
                    Message = content?.Message ?? "Error resetting password",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
