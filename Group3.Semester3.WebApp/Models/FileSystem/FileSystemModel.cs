using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Models.FileSystem
{
    public interface IFileSystemEntry
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        DirectoryEntry Parent { get; set; }

    }

    public class DirectoryEntry : IFileSystemEntry
    {
        [Required]
        [Display(Name = "File name")]
        public string Name { get ; set; }
        public Guid Id { get; set; }
        public DirectoryEntry Parent { get; set; }
    }

    public class FileEntry : IFileSystemEntry
    {
        [Required]
        [Display(Name = "File name")]
        public string Name { get; set; }
        public Guid Id { get; set; }
        public DirectoryEntry Parent { get; set; }
    }
}
