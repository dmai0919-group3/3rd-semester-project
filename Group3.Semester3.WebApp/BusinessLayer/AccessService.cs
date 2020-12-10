using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;
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
        // Permissions
        public const int Administrate = 128;
        public const int Manage = 32;
        public const int Write = 2;
        public const int Read = 1;

        /// <summary>
        /// This method checks if a user has access to a given file or not. If they have access, nothing happens and if they don't, an exception is thrown.
        /// </summary>
        /// <param name="user">The user whose permission we are checking</param>
        /// <param name="file">The FileEntity object we are checking the user's permissions on</param>
        /// <param name="accessLevelRequired">The access level required for certain operation</param>
        /// <exception cref="ValidationException">If the user doesn't have access to a given file, this exception is thrown.</exception>
        public void hasAccessToFile(UserModel user, FileEntity file, int accessLevelRequired);
        public void hasAccessToGroup(UserModel user, Group group, int accessLevelRequired = 1);
    }

    public class AccessService : IAccessService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ISharedFilesRepository _sharedFilesRepository;
        
        public AccessService(IFileRepository fileRepository, IGroupRepository groupRepository, ISharedFilesRepository sharedFilesRepository)
        {
            _fileRepository = fileRepository;
            _groupRepository = groupRepository;
            _sharedFilesRepository = sharedFilesRepository;
        }
        
        /// <summary>
        /// This method checks if a user has access to a given file or not. If they have access, nothing happens and if they don't, an exception is thrown.
        /// </summary>
        /// <param name="user">The user whose permission we are checking</param>
        /// <param name="file">The FileEntity object we are checking the user's permissions on</param>
        /// <param name="accessLevelRequired">The access level required for certain operation</param>
        /// <exception cref="ValidationException">If the user doesn't have access to a given file, this exception is thrown.</exception>
        public void hasAccessToFile(UserModel user, FileEntity file, int accessLevelRequired)
        {
            if (file.GroupId != Guid.Empty)
            {
                var group = _groupRepository.GetByGroupId(file.GroupId);
                
                var userGroup = _groupRepository.GetUserGroupModel(group.Id, user.Id);

                if (userGroup == null)
                {
                    throw new ValidationException("Operation Forbidden");
                }
                
                if ((userGroup.Permissions & accessLevelRequired) == 0)
                {
                    throw new ValidationException("Operation Forbidden");
                }
                
                return;
            }
            
            if (!user.Id.Equals(file.UserId))
            {
                // TODO: Replace with custom shared user permission
                if ((1 & accessLevelRequired) != 0)
                {
                    if (file.IsShared)
                    {
                        if (_sharedFilesRepository.IsSharedWithUser(file.Id, user.Id))
                        {
                            return;
                        }
                    }
                    else
                    {
                        // Check if parent folder is shared
                        // This is because files are not shared in the folder tree, only folders
                        if (!file.IsFolder && file.ParentId != Guid.Empty)
                        {
                            var parent = _fileRepository.GetById(file.ParentId);

                            if (_sharedFilesRepository.IsSharedWithUser(parent.Id, user.Id))
                            {
                                return;
                            }
                        }
                    }
                }
                
                throw new ValidationException("Operation forbidden.");
            }
        }
        
        public void hasAccessToGroup(UserModel user, Group group, int accessLevelRequired = 1)
        {
            var isInGroup = _groupRepository.IsUserInGroup(group.Id, user.Id);

            if (!isInGroup)
            {
                throw new ValidationException("Operation forbidden.");
            }
        }
    }
}
