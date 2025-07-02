using Models;
using Models.http;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Services
{
    public interface ITokenVerificationService
    {
        VerificationTokenResponse VerifyToken(string token, ApplicationDbContext context);
        Task<VerificationTokenRetryReponse> GenerateNewVerificationToken(User user, ApplicationDbContext context, IConfiguration configuration, IEmailService emailService);
    }

    public class TokenVerificationService : ITokenVerificationService
    {
        public async Task<VerificationTokenRetryReponse> GenerateNewVerificationToken(User user, ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            if (user.EmailConfirmed)
            {
                return new VerificationTokenRetryReponse
                {
                    Message = "Email already confirmed.",
                    StatusCode = 400
                };
            }

            EmailVerificationToken? existingToken = context.EmailVerificationTokens
                .FirstOrDefault(t => t.UserId == user.Id && t.ExpiresAt > DateTime.UtcNow);
            if (existingToken != null)
            {
                context.EmailVerificationTokens.Remove(existingToken);
                context.SaveChanges();
            }

            await Utils.GenerateNewVerificationToken(user, context, configuration, emailService);

            return new VerificationTokenRetryReponse
            {
                Message = "Verification token sent successfully.",
                StatusCode = 200
            };
        }

        public VerificationTokenResponse VerifyToken(string token, ApplicationDbContext context)
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

            EmailVerificationToken? emailVerificationTokenService = context.EmailVerificationTokens
                .FirstOrDefault(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);

            if (emailVerificationTokenService != null)
            {
                emailVerificationTokenService.ExpiresAt = DateTime.UtcNow;

                User? user = context.Users
                    .FirstOrDefault(u => u.Id == emailVerificationTokenService.UserId);

                if (user != null)
                {
                    user.EmailConfirmed = true;
                    context.Users.Update(user);
                }

                context.EmailVerificationTokens.Update(emailVerificationTokenService);
                context.SaveChanges();

                return new VerificationTokenResponse
                {
                    IsValid = true,
                    Message = "Token valid. Email confirmed.",
                    StatusCode = 200
                };
            }

            return new VerificationTokenResponse
            {
                IsValid = false,
                Message = "Invalid token.",
                StatusCode = 400
            };
        }
    }

}
