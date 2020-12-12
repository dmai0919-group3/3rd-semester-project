using System;
using System.Collections;
using System.Collections.Generic;
using Group3.Semester3.WebApp.Entities;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface ICommentRepository
    {
        public bool Insert(Comment comment);
        public bool Delete(Comment comment);
        public IEnumerable<Comment> GetByFileId(Guid fileId);
        public IEnumerable<Comment> GetByFileIdAndParentId(Guid fileId, Guid parentId);
    }
    
    public class CommentRepository : ICommentRepository
    {
        public bool Insert(Comment comment)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Comment comment)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Comment> GetByFileId(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Comment> GetByFileIdAndParentId(Guid fileId, Guid parentId)
        {
            throw new NotImplementedException();
        }
    }
}