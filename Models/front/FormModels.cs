using System.ComponentModel.DataAnnotations;

namespace Models.front
{
    public class ReportFormModel
    {
        [Required]
        public int Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
    }
}