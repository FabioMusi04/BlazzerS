using Models.enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        [Required, MaxLength(100)]
        public string Title { get; set; } = default!;

        [Required, MaxLength(500)]
        public string Message { get; set; } = default!;

        [Required]
        public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.Unread;

        [Required]
        public NotificationChannelEnum Channel { get; set; } = NotificationChannelEnum.App;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? DismissedAt { get; set; }

        public bool? EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }

        public bool? PushSent { get; set; }
        public DateTime? PushSentAt { get; set; }

        [MaxLength(1000)]
        public string? MetadataJson { get; set; }
    }
}
