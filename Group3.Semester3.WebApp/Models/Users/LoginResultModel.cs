using System;

namespace Group3.Semester3.WebApp.Models.Users
{
    public class LoginResultModel
    {
        public Guid Id { get; set; }
        
        public string Email { get; set; }
        
        public string Token { get; set; }
    }
}