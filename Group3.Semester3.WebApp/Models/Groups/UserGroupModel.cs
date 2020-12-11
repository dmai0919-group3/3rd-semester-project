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
        
        /*public short PermissionsNumber {
            get => this.Permissions.Permission;
        }
        
        public NewUserPermissions Permissions { get; set; }*/
    }
    
    public class NewUserPermissions
    {
        public bool HasAdministrate { get; set; }
        public bool HasManage { get; set; }
        public bool HasWrite { get; set; }
        public bool HasRead { get; set; }
        
        public short Permission {
            get 
            {
                short number = 0;
                if (HasAdministrate)
                {
                    number += Permissions.Administrate;
                }

                if (HasManage)
                {
                    number += Permissions.Manage;
                }

                if (HasWrite)
                {
                    number += Permissions.Write;
                }

                if (HasRead)
                {
                    number += Permissions.Read;
                }

                return number;
            }
        }
    }
}
