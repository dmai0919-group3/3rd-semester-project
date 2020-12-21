using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Entities
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return new string(Name);
        }
    }
}
