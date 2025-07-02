using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
using Models.front;
using Models.http;
using ReportingPortalServer.Services.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface IUserService
    {
        public UserResponse GetMeAsync(string JWT, ApplicationDbContext context);
        public UserResponse UpdateMeAsync(string JWT, UserPutModel updatedUser, ApplicationDbContext context);
        public Response UpdateMePasswordAsync(string JWT, string oldPassword, string newPassword, ApplicationDbContext context);
        public Response DeleteMeAsync(string JWT, ApplicationDbContext context);
        public UserResponse GetUserAsync(string JWT, int id, ApplicationDbContext context);
        public UserResponse UpdateUserAsync(string JWT, int id, UserPutModel updatedUser, ApplicationDbContext context);
        public Response DeleteUserAsync(string JWT, int id, ApplicationDbContext context);
        public UsersPaginatedResponse GetUserPaginationAsync(string JWT, UsersPaginatedRequest request, ApplicationDbContext context);
        public Task<Response> CreateResetPasswordRequestAsync(string email, ApplicationDbContext context, IConfiguration configuration, IEmailService emailService);
        public Response VerifyResetPasswordAsync(string token, ApplicationDbContext context);
        public Response ResetPasswordAsync(string token, string newPassword, ApplicationDbContext context);
    }

    public class UserService : IUserService
    {
        public UserResponse GetMeAsync(string JWT, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();

            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);

            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT user ID is not a valid integer."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Password = "baldman";
            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public UserResponse UpdateMeAsync(string JWT, UserPutModel updatedUser, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Role = updatedUser.Role;
            user.UpdatedAt = DateTime.UtcNow;

            context.SaveChanges();

            user.Password = "baldman";
            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public Response UpdateMePasswordAsync(string JWT, string oldPassword, string newPassword, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Old password is incorrect."
                };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            context.SaveChanges();

            user.Password = "baldman";
            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public Response DeleteMeAsync(string JWT, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            context.Users.Remove(user);
            context.SaveChanges();

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "Account eliminato con successo."
            };
        }

        public UserResponse GetUserAsync(string JWT, int id, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Password = "baldman";
            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public UserResponse UpdateUserAsync(string JWT, int id, UserPutModel updatedUser, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }
            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Role = updatedUser.Role;
            user.UpdatedAt = DateTime.UtcNow;
            context.SaveChanges();

            user.Password = "baldman";

            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public Response DeleteUserAsync(string JWT, int id, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            context.Users.Remove(user);
            context.SaveChanges();

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "User deleted successfully."
            };
        }

        public UsersPaginatedResponse GetUserPaginationAsync(string JWT, UsersPaginatedRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UsersPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UsersPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UsersPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UsersPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            bool asc = request.SortAscending ?? true;

            IQueryable<User> query = context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(u =>
                    u.Name.ToLower().Contains(request.Search.ToLower()) ||
                    u.Surname.ToLower().Contains(request.Search.ToLower()) ||
                    u.Email.ToLower().Contains(request.Search.ToLower()));
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.Role == request.Role.Value);
            }

            if (request.EmailConfirmed.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == request.EmailConfirmed.Value);
            }

            if (!string.IsNullOrEmpty(request.SortField))
            {
                if (asc)
                {
                    query = query.OrderBy(u => EF.Property<object>(u, request.SortField));
                }
                else
                {
                    query = query.OrderByDescending(u => EF.Property<object>(u, request.SortField));
                }
            }

            List<User> users = [.. query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsEnumerable()
                .Select(u => { u.Password = "baldman"; return u; })];

            return new UsersPaginatedResponse
            {
                Items = users,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = query.Count(),
                StatusCode = (int)System.Net.HttpStatusCode.OK
            };
        }

        public async Task<Response> CreateResetPasswordRequestAsync(string email, ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            User? user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            ResetPasswordToken? existingToken = context.PasswordResetTokens
              .FirstOrDefault(t => t.UserId == user.Id && t.ExpiresAt > DateTime.UtcNow);
            if (existingToken != null)
            {
                context.PasswordResetTokens.Remove(existingToken);
                context.SaveChanges();
            }

            await Utils.GenerateNewResetPasswordToken(user, context, configuration, emailService);

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "Reset password request created successfully."
            };
        }

        public Response VerifyResetPasswordAsync(string token, ApplicationDbContext context)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new UserResponse
                {
                    Message = "Token is required.",
                    StatusCode = 400
                };
            }

            ResetPasswordToken? resetPasswordToken = context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);

            if (resetPasswordToken != null)
            {
                resetPasswordToken.ExpiresAt = DateTime.UtcNow;

                User? user = context.Users
                    .FirstOrDefault(u => u.Id == resetPasswordToken.UserId);

                if (user != null)
                {
                    user.EmailConfirmed = true;
                    context.Users.Update(user);
                }

                context.PasswordResetTokens.Update(resetPasswordToken);
                context.SaveChanges();

                return new UserResponse
                {
                    Message = "Token valid.",
                    StatusCode = 200
                };
            }

            return new UserResponse
            {
                Message = "Invalid token.",
                StatusCode = 400
            };
        }

        public Response ResetPasswordAsync(string token, string newPassword, ApplicationDbContext context)
        {
            ResetPasswordToken? resetPasswordToken = context.PasswordResetTokens
                 .FirstOrDefault(t => t.Token == token && t.ExpiresAt < DateTime.UtcNow);

            if (resetPasswordToken == null)
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "Invalid or expired token."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == resetPasswordToken.UserId);
            if (user == null)
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.EmailConfirmed = true;
            context.Users.Update(user);
            context.PasswordResetTokens.Remove(resetPasswordToken);
            context.SaveChanges();

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "Password reset successfully."
            };
        }
    }
}
