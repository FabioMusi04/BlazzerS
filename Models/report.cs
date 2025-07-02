using Models.enums;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LocationDetail { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public ReportStatusEnum Status { get; set; } = ReportStatusEnum.Pending;

        [Required]
        public Point GeoPoint { get; set; } = new Point(0.0, 0.0) { SRID = 4326 };

        public int? FileId { get; set; } = null;

        public List<ReportReply> Replies { get; set; } = [];

        [ForeignKey(nameof(FileId))]
        public UploadFile? File { get; set; } = null;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; } = null;

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; } = null;
    }
}
