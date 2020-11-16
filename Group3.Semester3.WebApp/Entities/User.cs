using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Entities
{
    public class User
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public byte[] PasswordHash { get; set; }
        
        public byte[] PasswordSalt { get; set; }
    }
}
