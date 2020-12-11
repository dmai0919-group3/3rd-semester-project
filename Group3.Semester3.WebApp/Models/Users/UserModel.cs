using System;
using System.ComponentModel.DataAnnotations;

namespace Group3.Semester3.WebApp.Models.Users
{
    public class UserModel
    {
        public Guid Id { get; set; }
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
    }
}
