using System.ComponentModel.DataAnnotations;

namespace web_app.Models.Users
{
    public class AuthenticateModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public AuthenticateModel(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public AuthenticateModel()
        {
        }
    }
}