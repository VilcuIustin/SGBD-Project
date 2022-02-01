using System.ComponentModel.DataAnnotations;

namespace SGBD_Project.Models
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string WorkSpace { get; set; }
    }
}
