using System;

namespace Group3.Semester3.WebApp.Models.Groups
{
    public class AddUserGroupModel
    {
        public Guid GroupId { get; set; }
        
        public Guid UserId { get; set; }
        
        public string Email { get; set; }

        public bool HasAdministrate { get; set; }
        public bool HasManage { get; set; }
        public bool HasWrite { get; set; }
        public bool HasRead = true;
        
        public short Permissions {
            get 
            {
                short number = 0;
                if (HasAdministrate)
                {
                    number += BusinessLayer.Permissions.Administrate;
                }

                if (HasManage)
                {
                    number += BusinessLayer.Permissions.Manage;
                }

                if (HasWrite)
                {
                    number += BusinessLayer.Permissions.Write;
                }

                if (HasRead)
                {
                    number += BusinessLayer.Permissions.Read;
                }

                return number;
            }
        }
    }
}