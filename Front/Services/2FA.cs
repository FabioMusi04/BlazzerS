using Models.http;
using System.Net.Http.Json;

namespace Front.Services
{
    public class _2FAService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<_2FASetupResponse> GenerateQrCodeAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/Auth/2fa/setup", email);
                var content = await response.Content.ReadFromJsonAsync<_2FASetupResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new _2FASetupResponse
                {
                    Message = content?.Message ?? "Failed to generate QR code.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new _2FASetupResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> DisableAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/Auth/2fa/disable", email);
                var content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new Response
                {
                    Message = content?.Message ?? "Failed to disable 2FA.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> VerifyCodeAsync(string code, string email)
        {
            try
            {
                var payload = new { Code = code, Email = email };
                var response = await _http.PostAsJsonAsync("/api/Auth/2fa/verify", payload);
                var content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new Response
                {
                    Message = content?.Message ?? "Verification failed.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
