using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class ReportReplyService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<ReportRepliesPaginatedResponse> GetAllAsync(ReportsReplyPaginatedRequest request)
        {
            List<string> queryParams =
            [
                $"page={request.Page}",
                $"pageSize={request.PageSize}",
                $"reportId={request.ReportId}",
            ];

            string url = $"api/reportreply?{string.Join("&", queryParams)}";

            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                ReportRepliesPaginatedResponse? content = await response.Content.ReadFromJsonAsync<ReportRepliesPaginatedResponse>();

                return content != null && response.IsSuccessStatusCode
                    ? content
                    : new ReportRepliesPaginatedResponse
                    {
                        Message = content?.Message ?? "Failed to fetch reports.",
                        StatusCode = (int)response.StatusCode,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch reports: {ex.Message}");
                return new ReportRepliesPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }


        public async Task<ReportReplyResponse> GetByIdAsync(int id)
        {
            string url = $"api/reportreply/{id}";
            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                ReportReplyResponse? content = await response.Content.ReadFromJsonAsync<ReportReplyResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportReplyResponse
                    {
                        Message = content?.Message ?? "Failed to fetch report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch report: {ex.Message}");
                return new ReportReplyResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ReportReplyResponse> CreateAsync(MultipartFormDataContent model)
        {
            string url = "api/reportreply";
            try
            {
                HttpResponseMessage response = await _http.PostAsync(url, model);
                ReportReplyResponse? content = await response.Content.ReadFromJsonAsync<ReportReplyResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportReplyResponse
                    {
                        Message = content?.Message ?? "Failed to create report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create report: {ex.Message}");
                return new ReportReplyResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ReportReplyResponse> UpdateAsync(int id, MultipartFormDataContent model)
        {
            string url = $"api/reportreply/{id}";
            try
            {
                HttpResponseMessage response = await _http.PutAsync(url, model);
                ReportReplyResponse? content = await response.Content.ReadFromJsonAsync<ReportReplyResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportReplyResponse
                    {
                        Message = content?.Message ?? "Failed to update report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update report: {ex.Message}");
                return new ReportReplyResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ReportReplyResponse> DeleteAsync(int id)
        {
            string url = $"api/reportreply/{id}";
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync(url);
                ReportReplyResponse? content = await response.Content.ReadFromJsonAsync<ReportReplyResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportReplyResponse
                    {
                        Message = content?.Message ?? "Failed to update report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete report: {ex.Message}");
                return new ReportReplyResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
