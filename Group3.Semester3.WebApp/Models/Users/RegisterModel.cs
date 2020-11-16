using System.ComponentModel.DataAnnotations;

namespace web_app.Models.Users
{
    public class RegisterModel
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
    }
}