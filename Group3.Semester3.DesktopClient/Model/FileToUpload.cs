using System;
using System.Collections.Generic;
using System.Text;

namespace Group3.Semester3.DesktopClient.Model
{
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
