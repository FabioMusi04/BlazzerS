using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using WebPush;

namespace ReportingPortalServer.Services
{
    public interface INotificationService
    {
        Task<bool> SendNotificationPushUserAsync(int userId, string message, ApplicationDbContext context, IConfiguration configuration);
        NotificationsPaginatedResponse GetNotifications(string jwt, NotificationsPaginatedRequest request, ApplicationDbContext context);
        NotificationResponse ReadNotification(string jwt, int notificationId, ApplicationDbContext context);
        NotificationResponse CreateNotification(string jwt, CreateNotificationRequest request, ApplicationDbContext context);
        NotificationResponse DeleteNotification(string jwt, int notificationId, ApplicationDbContext context);
        NotificationResponse UpdateNotification(string jwt, int notificationId, string title, string message, ApplicationDbContext context);
    }

    public class NotificationService() : INotificationService
    {
        public async Task<bool> SendNotificationPushUserAsync(int userId, string message, ApplicationDbContext context, IConfiguration configuration)
        {
            User? user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            List<WebPush.PushSubscription> subscriptions = await context.PushSubscriptions
                .Where(s => s.UserId == userId)
                .Select(s => new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth))
                .ToListAsync();

            string? subject = configuration["VAPID:subject"];
            string? publicKey = configuration["VAPID:publicKey"];
            string? privateKey = configuration["VAPID:privateKey"];

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
            {
                Console.WriteLine("VAPID configuration is missing.");
                return false;
            }

            VapidDetails vapidDetails = new(subject, publicKey, privateKey);
            WebPushClient webPushClient = new();

            bool anySuccess = false;

            foreach (WebPush.PushSubscription subscription in subscriptions)
            {
                try
                {
                    await Task.Run(() => webPushClient.SendNotification(subscription, message, vapidDetails));
                    anySuccess = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending push notification: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine("Inner: " + ex.InnerException.Message);
                }
            }

            return anySuccess;
        }

        public NotificationsPaginatedResponse GetNotifications(string JWT, NotificationsPaginatedRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            List<Notification> notifications = [];
            int totalCount = 0;

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                notifications = [.. context.Notifications
                    .Where(n => n.UserId == parsedUserId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Include(n => n.User)
                ];

                totalCount = context.Notifications.Count(n => n.UserId == parsedUserId);
            }
            else
            {
                bool asc = request.SortAscending ?? true;

                IQueryable<Notification> query = context.Notifications.AsQueryable();

                if (request.Status.HasValue)
                {
                    query = query.Where(n => n.Status == request.Status.Value);
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(n =>
                        n.Title.ToLower().Contains(request.Search.ToLower()));
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

                notifications = [.. query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Include(n => n.User)
                    .AsEnumerable()
                    .Select(n => { n.User.Password = "baldman"; return n; })
                ];

                totalCount = context.Notifications.Count();
            }

            return new NotificationsPaginatedResponse
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        public NotificationResponse ReadNotification(string jwt, int notificationId, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            Notification? notification = context.Notifications.FirstOrDefault(n => n.Id == notificationId && n.UserId == parsedUserId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }

            notification.Status = NotificationStatusEnum.Read;
            notification.ReadAt = DateTime.UtcNow;

            context.SaveChanges();
            return new NotificationResponse
            {
                Notification = notification,
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification marked as read."
            };
        }
        public NotificationResponse CreateNotification(string jwt, CreateNotificationRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Not Authorized"
                };
            }
            Notification notification = new()
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Status = request.Status,
                Channel = request.Channel,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(notification);
            context.SaveChanges();
            return new NotificationResponse
            {
                Notification = notification,
                StatusCode = (int)HttpStatusCode.Created,
                Message = "Notification created successfully."
            };
        }
        public NotificationResponse DeleteNotification(string jwt, int notificationId, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }
            context.Notifications.Remove(notification);
            context.SaveChanges();
            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification deleted successfully."
            };
        }
        public NotificationResponse UpdateNotification(string jwt, int notificationId, string title, string message, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }
            notification.Title = title;
            notification.Message = message;
            context.SaveChanges();
            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification updated successfully.",
                Notification = notification
            };
        }
    }
}

