using Models.front;
using Models.http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Front.Services
{
    public class PostService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<PostsPaginatedResponse> GetAllAsync(PostPaginatedRequest request)
        {
            try
            {
                List<string> queryParams =
                [
                    $"page={request.Page}",
                    $"pageSize={request.PageSize}"
                ];

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
                HttpResponseMessage response = await _http.GetAsync($"api/Post?{queryString}");

                PostsPaginatedResponse? content = await response.Content.ReadFromJsonAsync<PostsPaginatedResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new PostsPaginatedResponse
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
                return new PostsPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Items = []
                };
            }
        }

        public async Task<PostsPaginatedResponse> GetMyAsync(PostPaginatedRequest request)
        {
            try
            {
                List<string> queryParams =
                [
                    $"page={request.Page}",
                    $"pageSize={request.PageSize}"
                ];

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
                HttpResponseMessage response = await _http.GetAsync($"api/Post/me?{queryString}");

                PostsPaginatedResponse? content = await response.Content.ReadFromJsonAsync<PostsPaginatedResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new PostsPaginatedResponse
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
                return new PostsPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Items = []
                };
            }
        }

        public async Task<PostResponse> GetByIdAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _http.GetAsync($"api/Post/{id}");
                PostResponse? content = await response.Content.ReadFromJsonAsync<PostResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new PostResponse
                {
                    Message = content?.Message ?? "Post not found",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new PostResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<PostResponse> CreateAsync(Stream fileStream, string fileName, string contentType, string postContent)
        {
            try
            {
                var content = new MultipartFormDataContent();

                if (fileStream == null || fileStream.Length == 0)
                {
                    content.Add(new StringContent(postContent ?? string.Empty), "content");

                    var response = await _http.PostAsync("api/Post", content);
                    var result = await response.Content.ReadFromJsonAsync<PostResponse>();

                    return result ?? new PostResponse
                    {
                        Message = "No response content",
                        StatusCode = (int)response.StatusCode
                    };
                }

                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");

                content.Add(fileContent, "file", fileName ?? "upload.dat");
                content.Add(new StringContent(postContent ?? string.Empty), "content");

                var responseWithFile = await _http.PostAsync("api/Post", content);
                var resultWithFile = await responseWithFile.Content.ReadFromJsonAsync<PostResponse>();

                return resultWithFile ?? new PostResponse
                {
                    Message = "No response content",
                    StatusCode = (int)responseWithFile.StatusCode
                };

            }
            catch (Exception ex)
            {
                return new PostResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }


        public async Task<PostResponse> UpdateAsync(int id, PostForm post)
        {
            try
            {
                HttpResponseMessage response = await _http.PutAsJsonAsync($"api/Post/{id}", post);
                PostResponse? content = await response.Content.ReadFromJsonAsync<PostResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new PostResponse
                {
                    Message = content?.Message ?? "Error updating post",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new PostResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> DeleteAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync($"api/Post/{id}");
                Response? content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new Response
                {
                    Message = content?.Message ?? "Error deleting post",
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
