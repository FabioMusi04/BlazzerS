using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class EmailVerificationTokenService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<VerificationTokenResponse?> VerifyTokenAsync(string token)
        {
            string url = $"api/EmailVerificationToken/{token}";
            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                VerificationTokenResponse? content = await response.Content.ReadFromJsonAsync<VerificationTokenResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    if (content.IsValid)
                    {
                        return content;
                    }

                    return new VerificationTokenResponse
                    {
                        Message = "The verification token is invalid or has expired.",
                        StatusCode = (int)response.StatusCode
                    };
                }

                return new VerificationTokenResponse
                {
                    Message = content?.Message ?? "An unknown error occurred.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new VerificationTokenResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<VerificationTokenRetryReponse?> RetryVerificationAsync(int userId)
        {
            string url = "api/EmailVerificationToken/resend";
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(url, new { UserId = userId });
                VerificationTokenRetryReponse? content = await response.Content.ReadFromJsonAsync<VerificationTokenRetryReponse>();
                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }
                return new VerificationTokenRetryReponse
                {
                    Message = content?.Message ?? "An unknown error occurred.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new VerificationTokenRetryReponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
