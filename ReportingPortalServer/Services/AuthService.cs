using Microsoft.IdentityModel.Tokens;
using Models;
using Models.http;
using ReportingPortalServer.Services.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface IAuthService
    {
        public LoginResponse LoginAsync(string email, string password, ApplicationDbContext context);
        public Task<RegisterResponse> RegisterAsync(RegisterRequest request, ApplicationDbContext context);
    }

    public class AuthService(IEmailService emailService, IConfiguration configuration) : IAuthService
    {
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;

        public LoginResponse LoginAsync(string email, string password, ApplicationDbContext context)
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

            user.Password = "baldman";

            if (!user.EmailConfirmed)
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    User = user,
                    Message = "Email not confirmed."
                };
            }

            string token = GenerateJwtToken(user);

            return new LoginResponse
            {
                User = user,
                Token = token,
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
                Name = request.Name,
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

        private static string GenerateJwtToken(User user)
        {
            byte[] key = new System.Text.UTF8Encoding().GetBytes("baldman_eroe_notturno_gey_che_combatte_contro_gli_etero");
            Claim[] claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            ];

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
