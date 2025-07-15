using Back.Services;
using Microsoft.AspNetCore.Mvc;
using Models.http;
using System.Collections.Concurrent;
using System.Text;
using OtpNet;
using QRCoder;

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

        #region 2fa

        [HttpPost("2fa/setup")]
        public IActionResult Setup2FA([FromBody] string email)
        {
            _logger.LogInformation("2FA setup requested for user: {Email}", email);

            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "Utente non trovato" });

            var key = KeyGeneration.GenerateRandomKey(20);
            var base32Key = Base32Encoding.ToString(key);

            user.TwoFactorSecretKey = base32Key;
            context.SaveChanges();

            var issuer = "Baltazar"; 
            var otpUrl = $"otpauth://totp/{issuer}:{email}?secret={base32Key}&issuer={issuer}&digits=6";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);

            var qrBase64 = Convert.ToBase64String(qrBytes);

            return Ok(new
            {
                secretKey = base32Key,
                qrCodeImage = $"data:image/png;base64,{qrBase64}"
            });
        }

        [HttpPost("2fa/verify")]
        public IActionResult Verify2FA([FromBody] TwoFACodeRequest input)
        {
            _logger.LogInformation("2FA verify requested for user: {Email}", input.Email);

            var user = context.Users.FirstOrDefault(u => u.Email == input.Email);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                return BadRequest(new { message = "2FA non inizializzata per questo utente" });

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecretKey));
            var isValid = totp.VerifyTotp(input.Code, out _, new VerificationWindow(2, 2));

            if (!isValid)
            {
                _logger.LogWarning("2FA code invalid for user: {Email}", input.Email);
                return BadRequest(new { message = "Codice 2FA non valido" });
            }

            user.IsTwoFactorEnabled = true;
            context.SaveChanges();

            _logger.LogInformation("2FA attivata per utente: {Email}", input.Email);
            return Ok(new { message = "2FA abilitata con successo" });
        }

        [HttpPost("2fa/disable")]
        public IActionResult Disable2FA([FromBody] Disable2FARequest request)
        {
            _logger.LogInformation("Richiesta disattivazione 2FA per: {Email}", request.Email);

            var user = context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new { message = "Utente non trovato" });

            if (!user.IsTwoFactorEnabled)
                return BadRequest(new { message = "La 2FA non è attiva per questo utente" });

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;

            context.SaveChanges();

            _logger.LogInformation("2FA disabilitata per: {Email}", request.Email);

            return Ok(new { message = "2FA disattivata con successo" });
        }

        #endregion
    }
}
