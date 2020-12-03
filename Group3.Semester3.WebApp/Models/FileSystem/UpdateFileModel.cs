using Group3.Semester3.WebApp.Models.Form;
using System;

namespace Group3.Semester3.WebApp.Models.FileSystem
{
    public class UpdateFileModel
    {
        public string Contents { get; set; }
        public Guid Id { get; set; }
        public FormVerification Form { get; set; }
    }
}