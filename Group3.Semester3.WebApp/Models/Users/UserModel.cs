using System;
using System.ComponentModel.DataAnnotations;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Helpers;

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

        public short PermissionsNumber { get; set; }

        public PermissionHelper Permissions {
            get
            {
                var helper = new PermissionHelper { PermissionsNumber = this.PermissionsNumber };

                return helper;
            }
        }
    }

    
}