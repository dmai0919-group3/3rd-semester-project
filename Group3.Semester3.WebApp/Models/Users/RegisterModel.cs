using System.ComponentModel.DataAnnotations;

namespace Group3.Semester3.WebApp.Models.Users
{
    public class RegisterModel
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
    }
}