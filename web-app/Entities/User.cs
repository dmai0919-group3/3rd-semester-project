using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web_app.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }
        
        public byte[] PasswordSalt { get; set; }
    }
}
