using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class NotificationService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<NotificationsPaginatedResponse> GetAllAsync(NotificationsPaginatedRequest request)
        {
            List<string> queryParams =
                [
                        $"page={request.Page}",
                        $"pageSize={request.PageSize}"
                ];

            if (request.Status.HasValue)
            {
                queryParams.Add($"status={request.Status.Value}");
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
            try
            {
                HttpResponseMessage response = await _http.GetAsync("api/Notification?" + queryString);
                NotificationsPaginatedResponse? content = await response.Content.ReadFromJsonAsync<NotificationsPaginatedResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationsPaginatedResponse
                    {
                        Message = content?.Message ?? "Failed to fetch notifications.",
                        StatusCode = (int)response.StatusCode,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch notifications: {ex.Message}");
                return new NotificationsPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }

        public async Task<NotificationConnectResponse> SubscribeToPushNotificationsAsync(string client, string endpoint, string p256dh, string auth)
        {
            string url = "api/Notification/connect-push-notifications";

            NotificationConnectRequest subscription = new()
            {
                Client = client,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            };

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(url, subscription);
                NotificationConnectResponse? content = await response.Content.ReadFromJsonAsync<NotificationConnectResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationConnectResponse
                    {
                        Message = content?.Message ?? "Failed to subscribe to push notifications.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new NotificationConnectResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<NotificationResponse> DeleteAsync(int notificationId)
        {
            string url = $"api/Notification/{notificationId}";
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync(url);
                NotificationResponse? content = await response.Content.ReadFromJsonAsync<NotificationResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationResponse
                    {
                        Message = content?.Message ?? "Failed to delete notification.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new NotificationResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
        {
            string url = "api/Notification";
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(url, request);
                NotificationResponse? content = await response.Content.ReadFromJsonAsync<NotificationResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationResponse
                    {
                        Message = content?.Message ?? "Failed to create notification.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new NotificationResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<NotificationResponse> MarkAsReadAsync(int notificationId)
        {
            string url = $"api/Notification/read/{notificationId}";
            try
            {
                HttpResponseMessage response = await _http.PutAsync(url, null);
                NotificationResponse? content = await response.Content.ReadFromJsonAsync<NotificationResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationResponse
                    {
                        Message = content?.Message ?? "Failed to mark notification as read.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new NotificationResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
