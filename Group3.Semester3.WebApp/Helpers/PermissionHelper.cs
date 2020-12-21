using Group3.Semester3.WebApp.BusinessLayer;
using Newtonsoft.Json;

namespace Group3.Semester3.WebApp.Helpers
{
    public class PermissionHelper
    {
        public bool HasAdministrate => (PermissionsNumber & Permissions.Administrate) != 0;
        public bool HasManage => (PermissionsNumber & Permissions.Manage) != 0;
        public bool HasWrite => (PermissionsNumber & Permissions.Write) != 0;
        public bool HasRead => (PermissionsNumber & Permissions.Read) != 0;

        public short PermissionsNumber { get; set; }
    }
}