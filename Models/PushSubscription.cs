using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class PushSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Client { get; set; } = default!;

        [Required]
        public string Endpoint { get; set; } = default!;

        [Required]
        public string P256dh { get; set; } = default!;

        [Required]
        public string Auth { get; set; } = default!;

        [Required]
        public int UserId { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;
    }
}