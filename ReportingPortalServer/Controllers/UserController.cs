using Microsoft.AspNetCore.Mvc;
using Models;
using Models.front;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(ILogger<UserController> logger, IUserService userService, IConfiguration configuration, IEmailService emailService, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IEmailService _emailService = emailService;
        private readonly ApplicationDbContext context = context;

        [HttpGet("me")]
        public UserResponse GetMe()
        {
            _logger.LogInformation("GetMe request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.GetMeAsync(jwt, context);
        }

        [HttpPut("me")]
        public UserResponse UpdateMe(UserPutModel user)
        {
            _logger.LogInformation("UpdateMe request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.UpdateMeAsync(jwt, user, context);
        }

        [HttpPut("me/password")]
        public Response UpdateMePassword(ChangePasswordFormModel changePasswordForm)
        {
            _logger.LogInformation("UpdateMePassword request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.UpdateMePasswordAsync(jwt, changePasswordForm.CurrentPassword, changePasswordForm.NewPassword, context);
        }

        [HttpDelete("me")]
        public Response DeleteMe()
        {
            _logger.LogInformation("DeleteMe request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.DeleteMeAsync(jwt, context);
        }

        [HttpGet("{userId}")]
        public UserResponse GetUser(int userId)
        {
            _logger.LogInformation($"GetUser request received for user ID: {userId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.GetUserAsync(jwt, userId, context);
        }

        [HttpGet]
        public PagedResponse<User> GetUserPagination([FromQuery] UsersPaginatedRequest request)
        {
            _logger.LogInformation($"GetUserPagination request received with parameters: Page={request.Page}, PageSize={request.PageSize}, Role={request.Role}, EmailConfirmed={request.EmailConfirmed}, Search={request.Search}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new PagedResponse<User>
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.GetUserPaginationAsync(jwt, request, context);
        }

        [HttpPut("{userId}")]
        public Response PutUser(int userId, UserPutModel user)
        {
            _logger.LogInformation($"PutUser request received for user ID: {userId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.UpdateUserAsync(jwt, userId, user, context);
        }

        [HttpDelete("{userId}")]
        public Response DeleteUser(int userId)
        {
            _logger.LogInformation($"DeleteUser request received for user ID: {userId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.DeleteUserAsync(jwt, userId, context);
        }

        [HttpPost("reset-password")]
        public Task<Response> CreateResetPassword(ResetPasswordRequest request)
        {
            _logger.LogInformation("Reset password request received");

            return _userService.CreateResetPasswordRequestAsync(request.Email, context, _configuration, _emailService);
        }

        [HttpGet("reset-password/{token}")]
        public Response VerifyToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new Response
                {
                    Message = "Token is required.",
                    StatusCode = 400
                };
            }

            return _userService.VerifyResetPasswordAsync(token, context);
        }

        [HttpPost("reset-password/{token}")]
        public Response ResetPassword(string token, ResetPasswordFormModel resetPasswordForm)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new Response
                {
                    Message = "Token is required.",
                    StatusCode = 400
                };
            }
            return _userService.ResetPasswordAsync(token, resetPasswordForm.NewPassword, context);
        }
    }
}
