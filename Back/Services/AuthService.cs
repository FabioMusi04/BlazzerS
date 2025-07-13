using Back.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Back.Services
{
    public interface IAuthService
    {
        public Task<LoginResponse> LoginAsync(string email, string password, ApplicationDbContext context, HttpContext httpContext);
        public Task<RegisterResponse> RegisterAsync(RegisterRequest request, ApplicationDbContext context);
        public Task<LoginResponse> RefreshAsync(ApplicationDbContext context, HttpContext httpContext);
        public Task<Response> LogoutAsync(string? deviceId, HttpContext httpContext, ApplicationDbContext context);
        public Task<Response> LogoutAllAsync(HttpContext httpContext, ApplicationDbContext context);
    }

    public class AuthService(IEmailService emailService, IConfiguration configuration) : IAuthService
    {
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<LoginResponse> LoginAsync(string email, string password, ApplicationDbContext context, HttpContext httpContext)
        {
            User? user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Not valid credentials"
                };
            }

            if (!user.EmailConfirmed)
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    User = user,
                    Message = "Email not confirmed."
                };
            }
            string jti = Guid.NewGuid().ToString();

            string token = GenerateJwtToken(user, jti);
            string refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;

            StringValues userAgent = httpContext.Request.Headers.UserAgent;
            string userAgentValue = userAgent.ToString() ?? "unknown";
            string deviceId = httpContext.Request.Headers["X-Device-ID"].ToString() ?? "unknown";
            string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var existingSession = await context.UserSessions
            .FirstOrDefaultAsync(s =>
                s.UserId == user.Id &&
                s.DeviceId == deviceId);

            if (existingSession != null)
            {
                existingSession.LastAccessedAt = DateTime.UtcNow;
                existingSession.IPAddress = ip;
                existingSession.UserAgent = userAgentValue;
                existingSession.Jti = jti;

                context.UserSessions.Update(existingSession);
            }
            else
            {
                UserSession newSession = new()
                {
                    UserId = user.Id,
                    IPAddress = ip,
                    UserAgent = userAgentValue,
                    LastAccessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    DeviceId = deviceId,
                    Jti = jti,
                };

                await context.UserSessions.AddAsync(newSession);
            }

            await context.SaveChangesAsync();

            user.Password = "baldman";
            user.RefreshToken = "baldman";

            return new LoginResponse
            {
                User = user,
                Token = token,
                RefreshToken = refreshToken,
                Message = "Login successful.",
                StatusCode = (int)System.Net.HttpStatusCode.OK
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, ApplicationDbContext context)
        {
            User? existingUser = context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Conflict,
                    Message = "Email already exists."
                };
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User user = new()
            {
                Email = request.Email,
                Password = hashedPassword,
                Name = Utils.SanitizeHtml(request.Name),
                Surname = request.Surname,
                Role = 0
            };

            context.Users.Add(user);
            context.SaveChanges();

            await Utils.GenerateNewVerificationToken(user, context, _configuration, _emailService);

            return new RegisterResponse
            {
                StatusCode = (int)System.Net.HttpStatusCode.Created,
                Message = "Registration successful. Please check your email to confirm your account.",
                UserId = user.Id,
            };
        }

        public async Task<LoginResponse> RefreshAsync(ApplicationDbContext context, HttpContext httpContext)
        {
            httpContext.Request.Cookies.TryGetValue("access_token", out var jwt);
            if (string.IsNullOrEmpty(jwt))
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            JwtSecurityTokenHandler handler = new();

            if (!handler.CanReadToken(jwt))
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(jwt);


            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT user ID is not a valid integer."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "User not found."
                };
            }

            httpContext.Request.Cookies.TryGetValue("refresh_token", out var old_token);

            if (user.RefreshToken != old_token)
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Refresh token is invalid or expired."
                };
            }

            string jti = Guid.NewGuid().ToString();
            string newToken = GenerateJwtToken(user, jti, 10080);
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;

            StringValues userAgent = httpContext.Request.Headers.UserAgent;
            string userAgentValue = userAgent.ToString() ?? "unknown";
            string deviceId = httpContext.Request.Headers["X-Device-ID"].ToString() ?? "unknown";
            string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var existingSession = await context.UserSessions
                .FirstOrDefaultAsync(s =>
                    s.UserId == user.Id &&
                    s.DeviceId == deviceId);

            if (existingSession != null)
            {
                existingSession.LastAccessedAt = DateTime.UtcNow;
                existingSession.IPAddress = ip;
                existingSession.UserAgent = userAgentValue;
                existingSession.Jti = jti;

                context.UserSessions.Update(existingSession);
            }
            else
            {
                UserSession newSession = new()
                {
                    UserId = user.Id,
                    IPAddress = ip,
                    UserAgent = userAgentValue,
                    LastAccessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    DeviceId = deviceId,
                    Jti = jti,
                };
                await context.UserSessions.AddAsync(newSession);
            }
            await context.SaveChangesAsync();

            user.Password = "baldman";
            user.RefreshToken = "baldman";

            return new LoginResponse
            {
                User = user,
                Token = newToken,
                RefreshToken = refreshToken,
                Message = "Login successful.",
                StatusCode = (int)System.Net.HttpStatusCode.OK
            };
        }

        public async Task<Response> LogoutAsync(string? devicePassedId, HttpContext httpContext, ApplicationDbContext context)
        {
            string? jwt = Utils.GetJwt(httpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            JwtSecurityTokenHandler tokenHandler = new();
            if (tokenHandler.ReadToken(jwt) is not SecurityToken token)
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Invalid JWT token."
                };
            }

            ClaimsPrincipal principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(new System.Text.UTF8Encoding().GetBytes("baldman_eroe_notturno_gey_che_combatte_contro_gli_etero")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            string deviceId = devicePassedId ?? httpContext.Request.Headers["X-Device-ID"].ToString() ?? "unknown";

            int userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var sessionsToRemove = context.UserSessions.Where(s => s.UserId == userId && deviceId == s.DeviceId).ToList();

            context.UserSessions.RemoveRange(sessionsToRemove);
            await context.SaveChangesAsync();
            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "Logout successful."
            };
        }

        public async Task<Response> LogoutAllAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            string? jwt = Utils.GetJwt(httpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            JwtSecurityTokenHandler tokenHandler = new();
            if (tokenHandler.ReadToken(jwt) is not SecurityToken token)
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Invalid JWT token."
                };
            }

            ClaimsPrincipal principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(new System.Text.UTF8Encoding().GetBytes("baldman_eroe_notturno_gey_che_combatte_contro_gli_etero")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            int userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var sessionsToRemove = context.UserSessions.Where(s => s.UserId == userId).ToList();
            context.UserSessions.RemoveRange(sessionsToRemove);
            await context.SaveChangesAsync();
            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "Logout from all devices successful."
            };
        }

        private static string GenerateJwtToken(User user, string jti, int duration = 15)
        {
            byte[] key = new System.Text.UTF8Encoding().GetBytes("baldman_eroe_notturno_gey_che_combatte_contro_gli_etero");
            Claim[] claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            ];

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(duration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
    }
}
