using System;
using System.Collections.Generic;
using System.Text;

namespace Group3.Semester3.DesktopClient.Model
{
    /// <summary>
    /// This model is used to store the files' name and path that we want to upload
    /// </summary>
    public class FileToUpload
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
