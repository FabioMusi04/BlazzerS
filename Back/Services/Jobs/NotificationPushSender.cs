using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
namespace ReportingPortalServer.Services.Jobs
{
    public class NotificationPushSender(IServiceScopeFactory scopeFactory, ILogger<NotificationPushSender> logger) : IScheduledJob
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<NotificationPushSender> _logger = logger;

        public string JobName => "PushNotificationSender";
        public TimeSpan Interval => TimeSpan.FromMinutes(3);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            List<Notification> pendingPushNotifications = await context.Notifications
                .Where(e => e.Channel == NotificationChannelEnum.Push && (e.PushSent == false || e.PushSent == null))
                .OrderBy(e => e.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Sending {Count} pending push notifications...", pendingPushNotifications.Count);

            foreach (Notification notification in pendingPushNotifications)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var user = await context.Users.FindAsync(new object[] { notification.UserId }, cancellationToken).ConfigureAwait(false);
                    if (user == null)
                    {
                        notification.PushSent = true;
                        context.Notifications.Update(notification);
                        continue;
                    }

                    await notificationService.SendNotificationPushUserAsync(user.Id, notification.Message, context, configuration).ConfigureAwait(false);

                    notification.PushSent = true;
                    notification.PushSentAt = DateTime.UtcNow;
                    context.Notifications.Update(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notification to user {UserId}", notification.UserId);
                    notification.PushSent = true;
                    notification.PushSentAt = DateTime.UtcNow;
                    context.Notifications.Update(notification);
                }
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
