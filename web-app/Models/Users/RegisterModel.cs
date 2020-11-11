using System.ComponentModel.DataAnnotations;

namespace web_app.Models.Users
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}