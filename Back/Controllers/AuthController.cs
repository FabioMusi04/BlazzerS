using Back.Services;
using Microsoft.AspNetCore.Mvc;
using Models.http;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ILogger<AuthController> logger, IAuthService authService, ApplicationDbContext context) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthService _authService = authService;
        private readonly ApplicationDbContext context = context;

        [HttpPost("login")]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            _logger.LogInformation("Login request received for user: {Username}", request.Email);

            HttpContext httpContext = this.HttpContext;

            LoginResponse response = await _authService.LoginAsync(request.Email, request.Password, context, httpContext);
            if (response.StatusCode != (int)System.Net.HttpStatusCode.OK)
            {
                _logger.LogWarning("Login failed for user: {Username}. Status code: {StatusCode}, Message: {Message}", request.Email, response.StatusCode, response.Message);
                return response;
            }

            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            Response.Cookies.Append("access_token", response?.Token, cookieOptions);

            response.Token = null;

            return response;
        }

        [HttpPost("register")]
        public Task<RegisterResponse> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register request received for user: {Email}", request.Email);

            return _authService.RegisterAsync(request, context);
        }

        [HttpPost("logout/{deviceId?}")]
        public async Task<Response> Logout(string? deviceId)
        {
            _logger.LogInformation("Logout request received");
            return await _authService.LogoutAsync(deviceId, HttpContext, context);
        }

        [HttpPost("logout/all")]
        public async Task<Response> LogoutAll()
        {
            _logger.LogInformation("LogoutAll request received");
            return await _authService.LogoutAllAsync(HttpContext, context);
        }
    }
}
