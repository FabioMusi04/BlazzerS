using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;

namespace ReportingPortalServer.Services.Jobs
{
    public class PendingReportReminderJob(IServiceScopeFactory scopeFactory, ILogger<PendingReportReminderJob> logger) : IScheduledJob
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<PendingReportReminderJob> _logger = logger;

        public string JobName => "PendingReportReminderJob";
        public TimeSpan Interval => TimeSpan.FromDays(1);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            DateTime cutoffDate = DateTime.UtcNow.AddDays(-7);

            var oldPendingReports = await context.Reports
                .Include(r => r.User)
                .Where(r => r.Status == ReportStatusEnum.Pending && r.CreatedAt <= cutoffDate)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} reports pending for more than 7 days.", oldPendingReports.Count);

            foreach (var report in oldPendingReports)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (report.User == null) continue;

                    string message = $"Reminder: Your report \"{report.Title}\" has been pending for over 7 days.";
                    await notificationService.SendNotificationPushUserAsync(report.User.Id, message, context, configuration);

                    context.Notifications.Add(new Notification
                    {
                        UserId = report.User.Id,
                        Message = message,
                        Channel = NotificationChannelEnum.Email,
                        CreatedAt = DateTime.UtcNow,
                        PushSent = true,
                        PushSentAt = DateTime.UtcNow
                    });

                    context.Notifications.Add(new Notification
                    {
                        UserId = report.User.Id,
                        Message = message,
                        Channel = NotificationChannelEnum.App,
                        CreatedAt = DateTime.UtcNow,
                        PushSent = true,
                        PushSentAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminder for report {ReportId}", report.Id);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
