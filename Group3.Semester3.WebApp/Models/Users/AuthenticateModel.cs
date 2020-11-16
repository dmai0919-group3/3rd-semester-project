using System.ComponentModel.DataAnnotations;

namespace Group3.Semester3.WebApp.Models.Users
{
    public class AuthenticateModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}