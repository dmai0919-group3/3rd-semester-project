using System;

namespace Group3.Semester3.WebApp.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid FileId { get; set; }
        public Guid ParentId { get; set; }
        public string Text { get; set; }
        public DateTime Sent { get; set; }
        
        public string Username { get; set; }
    }
}