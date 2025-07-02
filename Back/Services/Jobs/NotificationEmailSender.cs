using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
namespace ReportingPortalServer.Services.Jobs
{
    public class NotificationEmailSender(IServiceScopeFactory scopeFactory, ILogger<NotificationEmailSender> logger) : IScheduledJob
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<NotificationEmailSender> _logger = logger;

        public string JobName => "EmailNotificationSender";
        public TimeSpan Interval => TimeSpan.FromMinutes(2);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            List<Notification> pendingEmails = await context.Notifications
                .Where(e => e.Channel == NotificationChannelEnum.Email && (e.EmailSent == false || e.EmailSent == null))
                .OrderBy(e => e.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Sending {Count} pending emails...", pendingEmails.Count);

            foreach (Notification email in pendingEmails)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    User? user = await context.Users.FindAsync([email.UserId], cancellationToken).ConfigureAwait(false);
                    if (user == null)
                    {
                        email.EmailSent = true;
                        context.Notifications.Update(email);
                        continue;
                    }

                    await emailService.SendEmailAsync(user.Email, email.Title, email.Message).ConfigureAwait(false);

                    email.EmailSent = true;
                    email.EmailSentAt = DateTime.UtcNow;

                    context.Notifications.Update(email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to user {UserId}", email.UserId);
                    email.EmailSent = true;
                    email.EmailSentAt = DateTime.UtcNow;
                    context.Notifications.Update(email);
                }
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
