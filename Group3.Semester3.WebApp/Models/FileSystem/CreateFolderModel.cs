using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Models.FileSystem
{
    public class CreateFolderModel
    {
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public Guid GroupId { get; set; }
    }
}
