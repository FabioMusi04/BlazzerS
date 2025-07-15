using Back.Services;
using Microsoft.AspNetCore.Mvc;
using Models.http;
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
            Response.Cookies.Append("refresh_token", response?.RefreshToken, cookieOptions);

            response.Token = null;

            return response;
        }

        [HttpPost("register")]
        public Task<RegisterResponse> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register request received for user: {Email}", request.Email);

            return _authService.RegisterAsync(request, context);
        }

        [HttpPost("refresh")]
        public async Task<LoginResponse> Refresh()
        {
            HttpContext httpContext = this.HttpContext;
            LoginResponse response = await _authService.RefreshAsync(context, httpContext);

            if (response.StatusCode != (int)System.Net.HttpStatusCode.OK)
            {
                _logger.LogWarning("Refresh failed. Status code: {StatusCode}, Message: {Message}", response.StatusCode, response.Message);
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
            Response.Cookies.Append("refresh_token", response?.RefreshToken, cookieOptions);

            response.Token = null;
            return response;
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
        public _2FASetupResponse Setup2FA([FromBody] string email)
        {
            _logger.LogInformation("2FA setup requested for user: {Email}", email);

            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return new _2FASetupResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found"
                };

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

            return new _2FASetupResponse
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "2FA setup successfully",
                SecretKey = base32Key,
                QrCodeImage = $"data:image/png;base64,{qrBase64}"
            };
        }

        [HttpPost("2fa/verify")]
        public Response Verify2FA([FromBody] TwoFACodeRequest input)
        {
            _logger.LogInformation("2FA verify requested for user: {Email}", input.Email);

            var user = context.Users.FirstOrDefault(u => u.Email == input.Email);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found or 2FA not set up"
                };

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecretKey));
            var isValid = totp.VerifyTotp(input.Code, out _, new VerificationWindow(2, 2));

            if (!isValid)
            {
                _logger.LogWarning("2FA code invalid for user: {Email}", input.Email);
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "Invalid 2FA code"
                };
            }

            user.IsTwoFactorEnabled = true;
            context.SaveChanges();

            _logger.LogInformation("2FA attivata per utente: {Email}", input.Email);
            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "2FA enabled successfully"
            };
        }

        [HttpPost("2fa/disable")]
        public Response Disable2FA([FromBody] string email)
        {
            _logger.LogInformation("Richiesta disattivazione 2FA per: {Email}", email);

            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found"
                };

            if (!user.IsTwoFactorEnabled)
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "2FA is not enabled for this user"
                };

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;

            context.SaveChanges();

            _logger.LogInformation("2FA disabilitata per: {Email}", email);

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "2FA disabilitated succesfully"
            };
        }
        #endregion
    }
}
