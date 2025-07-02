using Models.enums;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$",
        ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, and one special character.")]
        public string Password { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Surname cannot be longer than 100 characters.")]
        [MinLength(2, ErrorMessage = "Surname must be at least 2 characters long.")]
        public string Surname { get; set; } = default!;

        public bool EmailConfirmed { get; set; } = false;

        public UserRoleEnum Role { get; set; } = UserRoleEnum.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}