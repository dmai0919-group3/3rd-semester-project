using System;
using System.Collections.Generic;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface ICommentService
    {
        public IEnumerable<Comment> GetComments(UserModel user, Guid fileId);
        public Comment CreateComment(UserModel user, Comment comment);
    }
    
    public class CommentService : ICommentService
    {
        private ICommentRepository _commentRepository;
        private IFileRepository _fileRepository;
        private IAccessService _accessService;
        
        public CommentService(ICommentRepository commentRepository, IFileRepository fileRepository, IAccessService accessService)
        {
            _commentRepository = commentRepository;
            _fileRepository = fileRepository;
            _accessService = accessService;
        }
        
        
        public IEnumerable<Comment> GetComments(UserModel user, Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Comment CreateComment(UserModel user, Comment comment)
        {
            var file = _fileRepository.GetById(comment.FileId);
            
            _accessService.hasAccessToFile(user, file, Permissions.Read);


            return comment;
        }
    }
}