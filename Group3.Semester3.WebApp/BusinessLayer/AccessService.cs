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
        public void hasAccess(UserModel user, FileEntity file);
    }

    public class AccessService : IAccessService
    {
        IFileRepository _fileRepository;
        public void hasAccess(UserModel user, FileEntity file)
        {
            if (!user.Id.Equals(file.UserId))
            {
                throw new ValidationException("Operation forbidden.");
            }
        }
    }
}
