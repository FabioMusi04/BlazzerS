using System.ComponentModel.DataAnnotations;

namespace Models.front
{
    public class RegisterFormModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$",
        ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, and one special character.")]
        public string Password { get; set; } = default!;

        [Required]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Surname cannot be longer than 100 characters.")]
        [MinLength(2, ErrorMessage = "Surname must be at least 2 characters long.")]
        public string Surname { get; set; } = default!;
    }
}
