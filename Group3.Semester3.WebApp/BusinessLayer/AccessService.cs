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
        public void hasAccess(UserModel user, FileEntity file);
    }

    public class AccessService : IAccessService
    {
        IFileRepository _fileRepository;

        /// <summary>
        /// This method checks if a user has access to a given file or not. If they have access, nothing happens and if they don't, an exception is thrown.
        /// </summary>
        /// <param name="user">The user whose permission we are checking</param>
        /// <param name="file">The FileEntity object we are checking the user's permissions on</param>
        /// <exception cref="ValidationException">If the user doesn't have access to a given file, this exception is thrown.</exception>
        public void hasAccess(UserModel user, FileEntity file)
        {
            if (!user.Id.Equals(file.UserId))
            {
                throw new ValidationException("Operation forbidden.");
            }
        }
    }
}
