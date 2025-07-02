using Microsoft.EntityFrameworkCore;
using Models;

namespace Back.Services
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<ResetPasswordToken> PasswordResetTokens { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<UploadFile> UploadFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
