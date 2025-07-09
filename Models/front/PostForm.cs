using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.front
{
    public class PostForm
    {
        [Required]
        [MaxLength(256)]
        public string Content { get; set; } = "";

        public IFormFile? Image { get; set; } = null;
    }
}
