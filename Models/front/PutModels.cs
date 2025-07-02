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
}
