using Microsoft.AspNetCore.Mvc;
using Models;
using Models.http;
using ReportingPortalServer.Services;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailVerificationTokenController(ITokenVerificationService tokenVerificationService, IConfiguration configuration, IEmailService emailService, ApplicationDbContext context) : ControllerBase
    {
        private readonly ITokenVerificationService _tokenVerificationService = tokenVerificationService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IEmailService _emailService = emailService;
        private readonly ApplicationDbContext _context = context;

        [HttpGet("{token}")]
        public VerificationTokenResponse VerifyToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new VerificationTokenResponse
                {
                    IsValid = false,
                    Message = "Token is required.",
                    StatusCode = 400
                };
            }

            return _tokenVerificationService.VerifyToken(token, _context);
        }

        [HttpPost("resend")]
        public async Task<VerificationTokenRetryReponse> RetrySendTokenAsync(VerificationTokenRetryRequest verificationTokenRetryRequest)
        {
            if (verificationTokenRetryRequest.UserId <= 0)
            {
                return new VerificationTokenRetryReponse
                {
                    Message = "Invalid user ID.",
                    StatusCode = 400
                };
            }

            User? user = _context.Users.FirstOrDefault(u => u.Id == verificationTokenRetryRequest.UserId);
            if (user == null)
            {
                return new VerificationTokenRetryReponse
                {
                    Message = "User not found.",
                    StatusCode = 404
                };
            }

            return await _tokenVerificationService.GenerateNewVerificationToken(user, _context, _configuration, _emailService);
        }
    }
}
