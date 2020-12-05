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

    }
    public class GroupService : IGroupService
    {
        private IGroupRepository _groupRepository;
        private IAccessService _accessService;

        public GroupService(IGroupRepository groupRepository, IAccessService accessService)
        {
            _groupRepository = groupRepository;
            _accessService = accessService;
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
    }
}
