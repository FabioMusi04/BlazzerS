using System.ComponentModel.DataAnnotations;

namespace Models.front
{
    public class LoginFormModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }
}
