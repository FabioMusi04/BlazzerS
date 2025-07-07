using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Back.Services;

public class JwtRevocationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context, ApplicationDbContext db)
    {

        string path = context.Request.Path.Value?.ToLower() ?? "";

        if (path != null && (
            path.StartsWith("/api/auth") ||
            path.StartsWith("/health") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/api/user/reset-password") ||
            path.StartsWith("/api/emailverificationtoken")
        ))
        {
            await _next(context);
            return;
        }

        string token = context.Request?.Cookies["access_token"]?.ToString() ?? "";

        if (!string.IsNullOrWhiteSpace(token))
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var jti = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (!string.IsNullOrEmpty(jti))
                {
                    var session = await db.UserSessions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Jti == jti);

                    if (session == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Token revoked or invalid.");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }

}
