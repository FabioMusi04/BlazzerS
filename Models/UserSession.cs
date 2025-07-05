using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class UserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(512)]
        public string UserAgent { get; set; } = string.Empty;

        [Required]
        [MaxLength(45)]
        public string IPAddress { get; set; } = string.Empty;

        [MaxLength(128)]
        public string DeviceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastAccessedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = default!;
    }
}