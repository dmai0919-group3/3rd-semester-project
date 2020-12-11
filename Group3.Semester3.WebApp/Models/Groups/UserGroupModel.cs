using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Helpers;

namespace Group3.Semester3.WebApp.Models.Groups
{
    public class UserGroupModel
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public short Permissions { get; set; }
    }
}
