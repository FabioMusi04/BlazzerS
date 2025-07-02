using Microsoft.AspNetCore.Mvc;
using Models;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController(ILogger<NotificationController> logger, ApplicationDbContext context, INotificationService notificationService) : Controller
    {
        private readonly ILogger<NotificationController> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly INotificationService _notificationService = notificationService;

        [HttpPost("connect-push-notifications")]
        public NotificationConnectResponse ConnectPushNotifications(NotificationConnectRequest request)
        {
            if (request == null)
            {
                return new NotificationConnectResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            User? user = _context.Users.FirstOrDefault(u => u.Email == request.Client);
            if (user == null)
            {
                return new NotificationConnectResponse
                {
                    Message = "User not found.",
                    StatusCode = 404,
                };
            }

            PushSubscription? existingSubscription = _context.PushSubscriptions
                .FirstOrDefault(s => s.Client == request.Client && s.Endpoint == request.Endpoint);

            if (existingSubscription == null)
            {
                PushSubscription subscription = new()
                {
                    Client = request.Client,
                    Endpoint = request.Endpoint,
                    P256dh = request.P256dh,
                    Auth = request.Auth,
                    UserId = user.Id
                };

                _context.PushSubscriptions.Add(subscription);
                _context.SaveChanges();

                return new NotificationConnectResponse
                {
                    Message = "Push notification subscription created successfully.",
                    StatusCode = 200,
                };
            }
            else
            {
                return new NotificationConnectResponse
                {
                    Message = "Subscription already exists.",
                    StatusCode = 409,
                };
            }
        }

        [HttpGet]
        public NotificationsPaginatedResponse GetNotifications([FromQuery] NotificationsPaginatedRequest request)
        {
            if (request == null)
            {
                return new NotificationsPaginatedResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            _logger.LogInformation($"GetNotificationPagination request received for page: {request.Page}, pageSize: {request.PageSize}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            NotificationsPaginatedResponse response = _notificationService.GetNotifications(jwt, request, _context);
            return response;
        }

        [HttpPut("read/{notificationId}")]
        public NotificationResponse MarkNotificationAsRead(int notificationId)
        {
            if (notificationId <= 0)
            {
                return new NotificationResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            _logger.LogInformation($"PutRead request received for notification: {notificationId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            NotificationResponse response = _notificationService.ReadNotification(jwt, notificationId, _context);

            return response;
        }

        [HttpPost]
        public NotificationResponse CreateNotification([FromBody] CreateNotificationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message) || request.UserId <= 0)
            {
                return new NotificationResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            _logger.LogInformation($"CreateNotification request received for userId: {request.UserId}, message: {request.Message}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            NotificationResponse response = _notificationService.CreateNotification(jwt, request, _context);

            return response;
        }

        [HttpDelete("{notificationId}")]
        public NotificationResponse DeleteNotification(int notificationId)
        {
            if (notificationId <= 0)
            {
                return new NotificationResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            _logger.LogInformation($"DeleteNotification request received for notificationId: {notificationId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            NotificationResponse response = _notificationService.DeleteNotification(jwt, notificationId, _context);

            return response;
        }

        [HttpPut]
        public NotificationResponse UpdateNotification([FromBody] UpdateNotificationRequest request)
        {
            if (request == null || request.NotificationId <= 0 || string.IsNullOrEmpty(request.Message))
            {
                return new NotificationResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }
            _logger.LogInformation($"UpdateNotification request received for notificationId: {request.NotificationId}, message: {request.Message}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            var response = _notificationService.UpdateNotification(jwt, request.NotificationId, request.Title, request.Message, _context);
            return new NotificationResponse
            {
                Message = "Notification updated successfully.",
                StatusCode = 200,
                Notification = response.Notification
            };
        }
    }
}
