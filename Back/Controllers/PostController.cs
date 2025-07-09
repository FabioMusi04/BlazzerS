using Back.Services;
using Back.Services.AppwriteIO;
using Back.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.front;
using Models.http;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController(ILogger<PostController> logger, IPostService postService, ApplicationDbContext context, IUploadFileService uploadFileService, IAppwriteClient appwriteClient) : Controller
    {
        private readonly ILogger<PostController> _logger = logger;
        private readonly IPostService _postService = postService;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IAppwriteClient _appwriteClient = appwriteClient;
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public PagedResponse<Post> GetAll([FromQuery] PostPaginatedRequest request)
        {
            _logger.LogInformation("GetPosts request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new PagedResponse<Post>
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _postService.GetAllPostsAsync(jwt, request, _context);
        }

        [HttpGet("me")]
        public PagedResponse<Post> GetAllMe([FromQuery] PostPaginatedRequest request)
        {
            _logger.LogInformation("GetAllMe request received for current user's posts");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new PagedResponse<Post>
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _postService.GetMyPostsAsync(jwt, request, _context);
        }

        [HttpGet("{postId}")]
        public PostResponse GetPost(int postId)
        {
            _logger.LogInformation($"GetPost request received for post ID: {postId}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new PostResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _postService.GetPostAsync(jwt, postId, _context);
        }

        [HttpPost]
        public async Task<PostResponse> CreatePost(PostForm postRequest)
        {
            _logger.LogInformation("CreatePost request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new PostResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _postService.CreatePostAsync(jwt, postRequest, _context, _uploadFileService, _appwriteClient);
        }

        [HttpPut("{postId}")]
        public async Task<Response> UpdatePost(int postId, PostForm postRequest)
        {
            _logger.LogInformation($"UpdatePost request received for post ID: {postId}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _postService.UpdatePostAsync(jwt, postId, postRequest, _context);
        }

        [HttpDelete("{postId}")]
        public Response DeletePost(int postId)
        {
            _logger.LogInformation($"DeletePost request received for post ID: {postId}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _postService.DeletePostAsync(jwt, postId, _context);
        }
    }
}
