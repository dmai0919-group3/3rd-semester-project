using System;

namespace Group3.Semester3.WebApp.Entities
{
    public class FileVersion
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public string AzureName { get; set; }
        public string Note { get; set; }
        public DateTime Created { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
    }
}