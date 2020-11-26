using System;
using System.ComponentModel.DataAnnotations;

namespace Group3.Semester3.WebApp.Models.Users
{
    public class LoginResultModel
    {
        public Guid Id { get; set; }
        
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        public string Token { get; set; }
    }
}