using System;
using System.Collections.Generic;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface ICommentService
    {
        public IEnumerable<Comment> GetComments(Guid fileId);
        public Comment CreateComment(UserModel model, Comment comment);
    }
    
    public class CommentService : ICommentService
    {
        private ICommentRepository _commentRepository;
        
        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        
        
        public IEnumerable<Comment> GetComments(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Comment CreateComment(UserModel model, Comment comment)
        {
            return comment;
        }
    }
}