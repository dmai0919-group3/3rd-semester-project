using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IAccessService
    {
        /// <summary>
        /// This method checks if a user has access to a given file or not. If they have access, nothing happens and if they don't, an exception is thrown.
        /// </summary>
        /// <param name="user">The user whose permission we are checking</param>
        /// <param name="file">The FileEntity object we are checking the user's permissions on</param>
        /// <exception cref="ValidationException">If the user doesn't have access to a given file, this exception is thrown.</exception>
        public void hasAccessToFile(UserModel user, FileEntity file);
        public bool hasAccessToGroup(UserModel user, Group group);
    }

    public class AccessService : IAccessService
    {
        IFileRepository _fileRepository;
        IGroupRepository _groupRepository;
        public AccessService(IFileRepository fileRepository, IGroupRepository groupRepository)
        {
            _fileRepository = fileRepository;
            _groupRepository = groupRepository;
        }
        
        /// <summary>
        /// This method checks if a user has access to a given file or not. If they have access, nothing happens and if they don't, an exception is thrown.
        /// </summary>
        /// <param name="user">The user whose permission we are checking</param>
        /// <param name="file">The FileEntity object we are checking the user's permissions on</param>
        /// <exception cref="ValidationException">If the user doesn't have access to a given file, this exception is thrown.</exception>
        public void hasAccessToFile(UserModel user, FileEntity file)
        {
            
            if (!user.Id.Equals(file.UserId))
            {
                throw new ValidationException("Operation forbidden.");
            }
        }

        public bool hasAccessToGroup(UserModel user, Group group)
        {
            var list = _groupRepository.GetByUserId(user.Id);

            foreach(var g in list) 
            {
                if(g.Id == group.Id)
                {
                    return true;
                }
            }

            throw new ValidationException("Operation forbidden.");
                                    
        }
    }
}
