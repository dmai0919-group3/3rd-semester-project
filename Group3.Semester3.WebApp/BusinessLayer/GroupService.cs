using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Groups;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IGroupService
    {
        public Group CreateGroup(UserModel user, CreateGroupModel model);
        public Group RenameGroup(Guid groupId, UserModel user, string name);
        public bool DeleteGroup(Guid groupId, UserModel user);
        public Group GetByGroupId(Guid groupId);
        public IEnumerable<Group> GetUserGroups(UserModel user);
        public IEnumerable<UserModel> GetGroupUsers(UserModel model, Guid groupId);
        public UserModel AddUser(UserModel user, AddUserGroupModel model);
        public UserModel UpdateUserPermissions(UserModel user, AddUserGroupModel model);
        public bool RemoveUser(UserModel user, UserGroupModel model);
        public UserModel GetUser(UserModel currentUser, string groupId, string userId);

    }
    public class GroupService : IGroupService
    {
        private IGroupRepository _groupRepository;
        private IAccessService _accessService;
        private IUserRepository _userRepository;

        public GroupService(IGroupRepository groupRepository, IAccessService accessService, IUserRepository userRepository)
        {
            _groupRepository = groupRepository;
            _accessService = accessService;
            _userRepository = userRepository;
        }


        public Group CreateGroup(UserModel user, CreateGroupModel model)
        {

            var group = new Group()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
            };

            var created = _groupRepository.Insert(group);

            if (!created)
            {
                throw new Exception(Messages.FailedToCreateGroup);
            }
            
            var userGroup = new AddUserGroupModel()
            {
                GroupId = group.Id, 
                UserId = user.Id,
                HasAdministrate = true,
                HasManage = true,
                HasRead = true,
                HasWrite = true
            };

            _groupRepository.AddUser(userGroup);

            return group;
        }

        public bool DeleteGroup(Guid groupId, UserModel user)
        {
            var group = _groupRepository.GetByGroupId(groupId);
            _accessService.hasAccessToGroup(user, group, Permissions.Administrate);
            var result = _groupRepository.Delete(groupId);

            if (!result)
            {
                throw new ValidationException(Messages.GroupNotExistsDeleted);
            }
            else return result;
        }

        public Group RenameGroup(Guid groupId, UserModel user, string name)
        {
            var group = GetByGroupId(groupId);
            _accessService.hasAccessToGroup(user, group, Permissions.Administrate);
            var result = _groupRepository.Rename(groupId, name);
            if (!result)
            {
                throw new ValidationException(Messages.GroupNotExistsRenamed);
            }
            else return GetByGroupId(groupId);
        }

        public Group GetByGroupId(Guid groupId)
        {
            var group = _groupRepository.GetByGroupId(groupId);
            if (group == null)
            {
                throw new ValidationException(Messages.GroupNotFound);
            }
            else return group;
        }

        public IEnumerable<Group> GetUserGroups(UserModel user)
        {
            var groups = _groupRepository.GetByUserId(user.Id);

            return groups;
        }

        public IEnumerable<UserModel> GetGroupUsers(UserModel user, Guid groupId)
        {
            var group = _groupRepository.GetByGroupId(groupId);
            _accessService.hasAccessToGroup(user, group);
            var users = _groupRepository.GetUsersByGroupId(groupId);

            return users;
        }

        private Guid ParseGuid(string guid)
        {
            Guid parsedGuid = Guid.Empty;

            try
            {
                parsedGuid = System.Guid.Parse(guid);
            }
            catch { }

            return parsedGuid;
        }

        public UserModel AddUser(UserModel user, AddUserGroupModel model)
        {
            var group = _groupRepository.GetByGroupId(model.GroupId);

            _accessService.hasAccessToGroup(user, group, Permissions.Administrate);
            var newUserEntity = _userRepository.GetByEmail(model.Email);
            
            if(newUserEntity != null)
            {
                var newUser = new UserModel() {Id= newUserEntity.Id, Name = newUserEntity.Name, Email = newUserEntity.Email};

                if (IsPartOfGroup(newUser, group))
                {
                    throw new ValidationException(Messages.UserAlreadyInGroup);
                }

                model.UserId = newUser.Id;

                var result = _groupRepository.AddUser(model);

                if (!result)
                {
                    throw new ValidationException(Messages.FailedToAddUser);
                }
                
                var userModel = _groupRepository.GetUserModel(group.Id, model.UserId);
                return userModel;
            }
            else
            {
                throw new ValidationException(Messages.UserNotFound);
            }
        }

        public UserModel UpdateUserPermissions(UserModel user, AddUserGroupModel model)
        {
            var group = _groupRepository.GetByGroupId(model.GroupId);

            _accessService.hasAccessToGroup(user, group, Permissions.Administrate);

            var result = _groupRepository.UpdatePermissions(model);
            
            if (!result)
            {
                throw new ValidationException(Messages.FailedToUpdatePermissions);
            }

            var affectedUser = _groupRepository.GetUserModel(model.GroupId, model.UserId);

            return affectedUser;
        }

        public bool RemoveUser(UserModel user, UserGroupModel model)
        {
            var group = _groupRepository.GetByGroupId(model.GroupId);

            if (user.Id != model.UserId)
            {
                _accessService.hasAccessToGroup(user, group, Permissions.Administrate);
            }
            
            return _groupRepository.RemoveUser(group.Id, model.UserId);
        }

        public UserModel GetUser(UserModel currentUser, string groupId, string userId)
        {
            var groupGuid = ParseGuid(groupId);
            var userGuid = ParseGuid(userId);
            
            var group = _groupRepository.GetByGroupId(groupGuid);

            _accessService.hasAccessToGroup(currentUser, group);

            if (userGuid == Guid.Empty)
            {
                userGuid = currentUser.Id;
            }
            
            var userModel = new UserModel() {Id = userGuid};

            if (IsPartOfGroup(userModel, group))
            {
                var user = _groupRepository.GetUserModel(groupGuid, userGuid);
                return user;
            }
            else
            {
                throw new ValidationException(Messages.UserNotInGroup);
            }
        }

        public bool IsPartOfGroup(UserModel user, Group group)
        {
            return _groupRepository.IsUserInGroup(group.Id, user.Id);
        }
    }
}
