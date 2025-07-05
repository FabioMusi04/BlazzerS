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
        public LoginResponse Login(LoginRequest request)
        {
            _logger.LogInformation("Login request received for user: {Username}", request.Email);

            LoginResponse response = _authService.LoginAsync(request.Email, request.Password, context);
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

            response.Token = null; // Clear token from response to avoid sending it in the response body

            return response;
        }

        [HttpPost("register")]
        public Task<RegisterResponse> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register request received for user: {Email}", request.Email);

            return _authService.RegisterAsync(request, context);
        }
    }
}
