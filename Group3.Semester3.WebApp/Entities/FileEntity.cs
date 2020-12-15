using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Entities
{
    public class FileEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public string AzureName { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public bool IsFolder { get; set; }
        public bool IsShared { get; set; }
        public DateTime Updated { get; set; }
        public long Size { get; set; }

        public FileEntity()
        {

        }
    }
}
