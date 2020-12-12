using Group3.Semester3.WebApp.BusinessLayer;
using Newtonsoft.Json;

namespace Group3.Semester3.WebApp.Helpers
{
    public class PermissionHelper
    {
        public bool HasAdministrate => (_permissionNumber & Permissions.Administrate) != 0;
        public bool HasManage => (_permissionNumber & Permissions.Manage) != 0;
        public bool HasWrite => (_permissionNumber & Permissions.Write) != 0;
        public bool HasRead => (_permissionNumber & Permissions.Read) != 0;

        private short _permissionNumber;
        
        public short PermissionsNumber
        {
            get
            {
                return _permissionNumber;
            }
            set => _permissionNumber = value;
        }
    }
}