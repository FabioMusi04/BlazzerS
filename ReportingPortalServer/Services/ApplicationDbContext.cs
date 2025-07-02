using Microsoft.EntityFrameworkCore;
using Models;

namespace ReportingPortalServer.Services
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<ResetPasswordToken> PasswordResetTokens { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UploadFile> UploadFiles { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportReply> ReportReplies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReportReply>()
                .HasOne(rr => rr.User)
                .WithMany()
                .HasForeignKey(rr => rr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReportReply>()
                .HasOne(rr => rr.Report)
                .WithMany(r => r.Replies)
                .HasForeignKey(rr => rr.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReportReply>()
                .HasOne(rr => rr.Attachment1)
                .WithMany()
                .HasForeignKey("Attachment2Id")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReportReply>()
                .HasOne(rr => rr.Attachment1)
                .WithMany()
                .HasForeignKey("Attachment3Id")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReportReply>()
                .HasOne(rr => rr.Attachment1)
                .WithMany()
                .HasForeignKey("Attachment1Id")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
