using System;
using System.Linq;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.AspNetCore.Http;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IUserService
    {
        /// <summary>
        /// This method is used to log a user into the system.
        /// </summary>
        /// <param name="model">The AuthenticateModel of the user</param>
        /// <returns>The UserModel of the logged in user</returns>
        /// <exception cref="ValidationException">If there were some errors with the entered credentials.</exception>
        UserModel Login(AuthenticateModel model);

        /// <summary>
        /// Gets a user by id
        /// </summary>
        /// <param name="id">The Guid of the user</param>
        /// <returns>A UserModel matching the given id</returns>
        /// <exception cref="ValidationException">If there are no users with the given id</exception>
        UserModel GetById(Guid id);

        /// <summary>
        /// Gets a user by email address
        /// TODO implement check if the user doesn't exist.
        /// </summary>
        /// <param name="email">The email address of a user</param>
        /// <returns>The UserModel matching the given email address</returns>
        UserModel GetByEmail(String email);

        /// <summary>
        /// Gets a user based on the HttpContext
        /// </summary>
        /// <param name="httpContext">The HttpContext of the request received</param>
        /// <returns>A UserModel matching the given identity</returns>
        /// <exception cref="ValidationException">If the user is not found</exception>
        UserModel GetFromHttpContext(dynamic httpContext);

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="model">A RegisterModel containing the users details</param>
        /// <returns>The UserModel object of the new user</returns>
        /// <exception cref="ValidationException">If there were some problems with the credentials given by the user.</exception>
        UserModel Register(RegisterModel model);

        /// <summary>
        /// Updates the information of an already existing user.
        /// TODO Implement this method
        /// </summary>
        /// <param name="userParam">The UserModel of the user with the new details included</param>
        /// <param name="password">The password of the user</param>
        /// <exception cref="NotImplementedException">This method is not implemented yet.</exception>
        User Update(UserUpdateModel userParam, UserModel currentUser);

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id">The Guid of the user to be deleted</param>
        /// <returns>True if the user has been deleted successfully.</returns>
        /// <exception cref="ValidationException">If the user does not exist or if there were some errors deleting the user.</exception>
        bool Delete(Guid id);
    }

    public class UserService : IUserService
    {
        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// This method is used to log a user into the system.
        /// </summary>
        /// <param name="model">The AuthenticateModel of the user</param>
        /// <returns>The UserModel of the logged in user</returns>
        /// <exception cref="ValidationException">If there were some errors with the entered credentials.</exception>
        public UserModel Login(AuthenticateModel model)
        {
            var email = model.Email;
            var password = model.Password;

            if (!IsValidEmail(email))
            {
                throw new ValidationException(Messages.EmailInvalid);
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ValidationException(Messages.EmailEmpty);

            var user = _userRepository.GetByEmail(email);

            // check if username exists
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                throw new ValidationException(Messages.IncorrectInitials);

            // authentication successful, return user

            return new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }
        
        /// <summary>
        /// Gets a user by id
        /// </summary>
        /// <param name="id">The Guid of the user</param>
        /// <returns>A UserModel matching the given id</returns>
        /// <exception cref="ValidationException">If there are no users with the given id</exception>
        public UserModel GetById(Guid id)
        {
            var user = _userRepository.Get(id);
            if (user != null)
            {
                return new UserModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name
                };
            }
            else throw new ValidationException(Messages.UserNotFound);
        }

        /// <summary>
        /// Gets a user based on the HttpContext
        /// </summary>
        /// <param name="httpContext">The HttpContext of the request received</param>
        /// <returns>A UserModel matching the given identity</returns>
        /// <exception cref="ValidationException">If the user is not found</exception>
        public UserModel GetFromHttpContext(dynamic httpContext)
        {
            var identityString = httpContext.User.Identity.Name;
            
            if (identityString == null)
            {
                return null;
            }
            
            var userId = new Guid(identityString);

            var user = GetById(userId);
            if (user == null)
            {
                throw new ValidationException(Messages.UserNotFound);
            }
            else return user;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="model">A RegisterModel containing the users details</param>
        /// <returns>The UserModel object of the new user</returns>
        /// <exception cref="ValidationException">If there were some problems with the credentials given by the user.</exception>
        public UserModel Register(RegisterModel model)
        {
            // validation
            if (string.IsNullOrWhiteSpace(model.Password))
                throw new ValidationException(Messages.PasswordIsRequired);

            var password = model.Password.Trim();

            if (password.Length < 8)
            {
                throw new ValidationException(Messages.PasswordTooShort);
            }

            if (!IsValidEmail(model.Email))
            {
                throw new ValidationException(Messages.EmailInvalid);
            }
            
            var dbUser = _userRepository.GetByEmail(model.Email);
            
            if (dbUser != null)
                throw new ValidationException(Messages.UserAlreadyExists);

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            User user = new User() { Email = model.Email, Name = model.Name };

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            bool success = _userRepository.Insert(user);

            if (!success)
                throw new ValidationException(Messages.UserNotCreated);

            return new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }

        /// <summary>
        /// Updates the information of an already existing user.
        /// TODO Implement this method
        /// </summary>
        /// <param name="userParam">The UserModel of the user with the new details included</param>
        /// <exception cref="ValidationException">invalid text in the form</exception>
        public User Update(UserUpdateModel userParam, UserModel currentUser)
        {
            var user = _userRepository.Get(currentUser.Id);

            if(currentUser.Id != user.Id)
            {
                throw new ValidationException(Messages.OperationForbidden);
            }
            if (user == null)
                throw new ValidationException(Messages.UserNotFound);

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Name) && userParam.Name != user.Name)
            {
                user.Name = userParam.Name;
            }

            // update password if provided
            if (!string.IsNullOrWhiteSpace(userParam.NewPassword) && !string.IsNullOrWhiteSpace(userParam.NewPasswordCheck))
            {
                if(string.IsNullOrWhiteSpace(userParam.OldPassword))
                {
                    throw new ValidationException(Messages.OldPasswordEmpty);
                }
                
                if (userParam.NewPassword.Equals(userParam.NewPasswordCheck)) {
                    
                    if(!VerifyPasswordHash(userParam.OldPassword, user.PasswordHash, user.PasswordSalt))
                    {
                        throw new ValidationException(Messages.WrongPassword);
                    }
                    
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(userParam.NewPassword, out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }
                else
                {
                    throw new ValidationException(Messages.PasswordsNotMatching);
                }
            }

            var result = _userRepository.Update(user);
            if (!result)
            {
                throw new ValidationException(Messages.UserNotExistsAltered);
            }
            else return user;
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id">The Guid of the user to be deleted</param>
        /// <returns>True if the user has been deleted successfully.</returns>
        /// <exception cref="ValidationException">If the user does not exist or if there were some errors deleting the user.</exception>
        public bool Delete(Guid id)
        {

            bool result = _userRepository.Delete(id);
            if (!result)
            {
                throw new ValidationException(Messages.UserNotExistsDeleted);
            }
            else return result;

        }
        
        /// <summary>
        /// Gets a user by email address
        /// TODO implement check if the user doesn't exist.
        /// </summary>
        /// <param name="email">The email address of a user</param>
        /// <returns>The UserModel matching the given email address</returns>
        public UserModel GetByEmail(string email)
        {
            try
            {
                var user = _userRepository.GetByEmail(email);
                return new UserModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #region Private helper methods

        /// <summary>
        /// Generates a password hash
        /// </summary>
        /// <param name="password">The password that needs to be hashed</param>
        /// <param name="passwordHash">The hash used</param>
        /// <param name="passwordSalt">The salt used</param>
        /// <exception cref="ArgumentNullException">If the entered password, hash or salt is empty or invalid.</exception>
        /// <exception cref="ArgumentException">If the given password is only whitespace or empty.</exception>
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException(Messages.PasswordEmpty);
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Verifies a password hash
        /// </summary>
        /// <param name="password">The password that needs to be checked</param>
        /// <param name="storedHash">The hash used</param>
        /// <param name="storedSalt">The salt used</param>
        /// <returns>True if the verification passes</returns>
        /// <exception cref="ArgumentNullException">If the entered password, hash or salt is empty or invalid.</exception>
        /// <exception cref="ArgumentException">If the entered password, hash or salt is empty or invalid.</exception>
        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException(Messages.PasswordEmpty);
            }

            if (storedHash.Length != 64) 
                throw new ArgumentException(Messages.PasswordHashLenghtInvalid, "passwordHash");
            
            if (storedSalt.Length != 128) 
                throw new ArgumentException(Messages.PasswordSaltLenghtInvalid, "passwordSalt");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
        
        private bool IsValidEmail(string email)
        {
            try {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch {
                return false;
            }
        }
        
        #endregion
    }
}
