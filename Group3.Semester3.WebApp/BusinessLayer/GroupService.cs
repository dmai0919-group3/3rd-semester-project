using Group3.Semester3.WebApp.Entities;
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
        public UserModel AddUser(string userMail, UserGroupModel model);
        public bool RemoveUser(UserModel user, UserGroupModel model);

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
                throw new Exception("Failed to create group");
            }

            _groupRepository.AddUser(group.Id, user.Id);

            return group;
        }

        public bool DeleteGroup(Guid groupId, UserModel user)
        {
            var group = _groupRepository.GetByGroupId(groupId);
            _accessService.hasAccessToGroup(user, group);
            var result = _groupRepository.Delete(groupId);

            if (!result)
            {
                throw new ValidationException("Group non-existent or not deleted.");
            }
            else return result;
        }

        public Group RenameGroup(Guid groupId, UserModel user, string name)
        {
            var group = GetByGroupId(groupId);
            _accessService.hasAccessToGroup(user, group);
            var result = _groupRepository.Rename(groupId, name);
            if (!result)
            {
                throw new ValidationException("Group non-existent or not renamed.");
            }
            else return GetByGroupId(groupId);
        }

        public Group GetByGroupId(Guid groupId)
        {
            var group = _groupRepository.GetByGroupId(groupId);
            if (group == null)
            {
                throw new ValidationException("No group found.");
            }
            else return group;
        }

        public IEnumerable<Group> GetUserGroups(UserModel user)
        {
            var groups = _groupRepository.GetByUserId(user.Id);

            return groups;
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

        public UserModel AddUser(string userMail, UserGroupModel model)
        {
            var group = _groupRepository.GetByGroupId(model.GroupId);
            var user = _userRepository.GetByEmail(userMail);
            UserModel userModel = new UserModel() { Email = user.Email, Id = user.Id, Name = user.Name };

            if(_accessService.hasAccessToGroup(userModel, group))
            {
                var newUserEntity = _userRepository.Get(model.UserId);

                var newUser = new UserModel() {Id= newUserEntity.Id};

                if (IsPartOfGroup(newUser, group))
                {
                    throw new ValidationException("User is already part of the group");
                }

                var result = _groupRepository.AddUser(group.Id, newUser.Id);
                if(result)
                {
                    return userModel;
                }
                else
                {
                    throw new ValidationException("User not found");
                }

            } else
            {
                throw new ValidationException("Operation forbidden");
            }
        }

        public bool RemoveUser(UserModel user, UserGroupModel model)
        {
            var group = _groupRepository.GetByGroupId(model.GroupId);

            if (_accessService.hasAccessToGroup(user, group))
            {
                return _groupRepository.RemoveUser(group.Id, model.UserId);
            }
            else
            {
                throw new ValidationException("Operation forbidden");
            }
        }

        public bool IsPartOfGroup(UserModel user, Group group)
        {
            try
            {
                _accessService.hasAccessToGroup(user, group);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
