using Back.Services;
using Microsoft.AspNetCore.Mvc;
using Models.http;
using System.Collections.Concurrent;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ILogger<AuthController> logger, IAuthService authService, ApplicationDbContext context) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthService _authService = authService;
        private readonly ApplicationDbContext context = context;

        // Configurazione rate limit
        private static readonly ConcurrentDictionary<string, (int Attempts, DateTime? BlockedUntil)> _loginAttempts = new();
        private const int MaxAttempts = 5;
        private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(10);

        [HttpPost("login")]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            _logger.LogInformation("Login request received for user: {Username}", request.Email);

            string key = request.Email.ToLowerInvariant();
            var now = DateTime.UtcNow;

            // Controllo blocco
            if (_loginAttempts.TryGetValue(key, out var entry) && entry.BlockedUntil.HasValue && entry.BlockedUntil > now)
            {
                return new LoginResponse
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = $"Troppi tentativi falliti. Riprova dopo le {entry.BlockedUntil.Value:HH:mm:ss} UTC."
                };
            }

            HttpContext httpContext = this.HttpContext;
            LoginResponse response = await _authService.LoginAsync(request.Email, request.Password, context, httpContext);

            if (response.StatusCode != (int)System.Net.HttpStatusCode.OK)
            {
                // Incrementa tentativi falliti
                int attempts = entry.Attempts + 1;
                DateTime? blockedUntil = null;
                if (attempts >= MaxAttempts)
                {
                    blockedUntil = now.Add(BlockDuration);
                    attempts = 0; // resetta tentativi dopo blocco
                }
                _loginAttempts[key] = (attempts, blockedUntil);

                _logger.LogWarning("Login failed for user: {Username}. Status code: {StatusCode}, Message: {Message}", request.Email, response.StatusCode, response.Message);
                return response;
            }

            // Login riuscito: resetta tentativi
            _loginAttempts.TryRemove(key, out _);

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
