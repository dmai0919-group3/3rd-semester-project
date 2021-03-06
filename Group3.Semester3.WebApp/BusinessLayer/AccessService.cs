﻿using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using System;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public static class Permissions
    {
        // Permissions
        public const short Administrate = 128;
        public const short Manage = 32;
        public const short Write = 2;
        public const short Read = 1;
    }
    
    public interface IAccessService
    {
        /// <summary>
        /// This method checks if a user has access to a given file or not. If they have access, nothing happens and if they don't, an exception is thrown.
        /// </summary>
        /// <param name="user">The user whose permission we are checking</param>
        /// <param name="file">The FileEntity object we are checking the user's permissions on</param>
        /// <param name="accessLevelRequired">The access level required for certain operation</param>
        /// <exception cref="ValidationException">If the user doesn't have access to a given file, this exception is thrown.</exception>
        public void HasAccessToFile(UserModel user, FileEntity file, int accessLevelRequired);
        public void HasAccessToGroup(UserModel user, Group group, int accessLevelRequired = 1);
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
        public void HasAccessToFile(UserModel user, FileEntity file, int accessLevelRequired)
        {
            if (file.GroupId != Guid.Empty)
            {
                var group = _groupRepository.GetByGroupId(file.GroupId);
                
                var userGroup = _groupRepository.GetUserGroupModel(group.Id, user.Id);

                if (userGroup == null)
                {
                    throw new ValidationException(Messages.OperationForbidden);
                }
                
                if ((userGroup.Permissions & accessLevelRequired) == 0)
                {
                    throw new ValidationException(Messages.OperationForbidden);
                }
                
                return;
            }
            
            if (user.Id != file.UserId)
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
                        // Check if some of the parent folders are shared
                        // This is because files are not shared in the folder tree, only one main parent folder
                        if (file.ParentId != Guid.Empty)
                        {
                            var parents = _fileRepository.GetParents(file.Id);

                            if (_sharedFilesRepository.IsSharedWithUser(parents, user.Id))
                            {
                                return;
                            }
                        }
                    }
                }
                
                throw new ValidationException(Messages.OperationForbidden);
            }
        }
        
        public void HasAccessToGroup(UserModel user, Group group, int accessLevelRequired = 1)
        {
            var userGroupModel = _groupRepository.GetUserGroupModel(group.Id, user.Id);
            
            if (userGroupModel == null)
            {
                throw new ValidationException(Messages.OperationForbidden);
            }
            
            if ((userGroupModel.Permissions & accessLevelRequired) == 0)
            {
                throw new ValidationException(Messages.OperationForbidden);
            }
        }
    }
}
