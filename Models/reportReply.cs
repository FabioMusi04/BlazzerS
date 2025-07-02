using Models.enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ReportReply
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public ReportStatusEnum NewStatus { get; set; }

        public int? Attachment1Id { get; set; }
        public int? Attachment2Id { get; set; }
        public int? Attachment3Id { get; set; }

        [ForeignKey(nameof(Attachment1Id))]
        public UploadFile? Attachment1 { get; set; }

        [ForeignKey(nameof(Attachment2Id))]
        public UploadFile? Attachment2 { get; set; }

        [ForeignKey(nameof(Attachment3Id))]
        public UploadFile? Attachment3 { get; set; }

        [ForeignKey(nameof(ReportId))]
        public Report Report { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;
    }
}
