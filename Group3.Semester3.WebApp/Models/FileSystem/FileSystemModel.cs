using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Models.FileSystem
{
    public interface IFileSystemEntry
    {
        public string Name { get; set; }
        public Guid UUID { get; set; }
        DirectoryEntry Parent { get; set; }

    }

    public class DirectoryEntry : IFileSystemEntry
    {
        public string Name { get ; set; }
        public Guid UUID { get; set; }
        public DirectoryEntry Parent { get; set; }
    }

    public class FileEntry : IFileSystemEntry
    {
        public string Name { get; set; }
        public Guid UUID { get; set; }
        public DirectoryEntry Parent { get; set; }
    }
}
