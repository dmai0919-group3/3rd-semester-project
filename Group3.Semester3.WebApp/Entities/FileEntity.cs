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
        public string AzureName { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public bool IsFolder { get; set; }
        public DateTime Updated { get; set; }
    }
}
