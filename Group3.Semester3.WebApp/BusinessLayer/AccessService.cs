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
        public void hasAccessToFile(UserModel user, FileEntity file);
        public bool hasAccessToGroup(UserModel user, Group group);
    }

    public class AccessService : IAccessService
    {
        IFileRepository _fileRepository;
        IGroupRepository _groupRepository;
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

            if (list.Contains(group))
            {
                return true;
            }
            else
            {
                throw new ValidationException("Operation forbidden.");
            }
                        
        }
    }
}
