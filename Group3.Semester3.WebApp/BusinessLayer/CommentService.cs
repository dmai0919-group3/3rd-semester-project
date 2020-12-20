using System;
using System.Collections.Generic;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface ICommentService
    {
        public IEnumerable<Comment> GetComments(UserModel user, Guid fileId);
        public Comment CreateComment(UserModel user, Comment comment);
        public bool AddToGroup(UserModel user, Guid fileId);
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
            var file = _fileRepository.GetById(fileId);
            _accessService.HasAccessToFile(user, file, Permissions.Read);
            var comments = _commentRepository.GetByFileIdAndParentId(fileId, Guid.Empty);
            return comments;
        }

        public Comment CreateComment(UserModel user, Comment comment)
        {
            var file = _fileRepository.GetById(comment.FileId);
            
            _accessService.HasAccessToFile(user, file, Permissions.Read);

            comment.Id = Guid.NewGuid();
            comment.Sent = DateTime.Now;
            comment.UserId = user.Id;
            comment.Username = user.Name;

            if (_commentRepository.Insert(comment))
            {
                return comment;
            }
            else
            {
                throw new ValidationException("Failed to create comment");
            }
        }

        public bool AddToGroup(UserModel user, Guid fileId)
        {
            var file = _fileRepository.GetById(fileId);
            
            _accessService.HasAccessToFile(user, file, Permissions.Read);

            return true;
        }
    }
}