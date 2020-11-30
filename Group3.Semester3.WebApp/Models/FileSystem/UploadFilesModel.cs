using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Group3.Semester3.WebApp.Models.FileSystem
{
    public class UploadFilesModel
    {
        public List<IFormFile> Files { get; set; }
        public string ParentId { get; set; }
    }
}