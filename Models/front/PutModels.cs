using Models.enums;
using System.ComponentModel.DataAnnotations;

namespace Models.front
{
    public class UserPutModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Surname cannot be longer than 100 characters.")]
        [MinLength(2, ErrorMessage = "Surname must be at least 2 characters long.")]
        public string Surname { get; set; } = default!;

        public UserRoleEnum Role { get; set; } = UserRoleEnum.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotificationPutModel
    {
        public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.Unread;
        public DateTime? ReadAt { get; set; }
        public DateTime? DismissedAt { get; set; }
        public bool? EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public bool? PushSent { get; set; }
        public DateTime? PushSentAt { get; set; }
    }

    public class DailyReportCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }

    }

    public class ReportsPerCategory
    {
        public string Category { get; set; } = null!;
        public int Count { get; set; }
    }

    public class TopUserReportCount
    {
        public string UserId { get; set; } = string.Empty;
        public int Count { get; set; }
    }

}
