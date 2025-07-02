using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;

namespace ReportingPortalServer.Controllers
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

            return _authService.LoginAsync(request.Email, request.Password, context);
        }

        [HttpPost("register")]
        public Task<RegisterResponse> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register request received for user: {Email}", request.Email);

            return _authService.RegisterAsync(request, context);
        }
    }
}
