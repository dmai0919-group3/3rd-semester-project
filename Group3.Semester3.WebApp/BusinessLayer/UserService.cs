using System;
using System.Linq;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.AspNetCore.Http;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    // interface for a User service
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
        // getting an instance of a user repository to be able to communicate with the db layer
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

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ValidationException("Email or password cannot be empty.");

            var user = _userRepository.GetByEmail(email);

            // check if username exists
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                throw new ValidationException("Incorrect email or password.");

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
            else throw new ValidationException("No user found.");
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
                throw new ValidationException("User not found");
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
                throw new ValidationException("Password is required");

            var dbUser = _userRepository.GetByEmail(model.Email);

            // TODO: how to validate this so that we prevent hacker from knowing this 
            // email is in db and bruteforcing? *Mogens said this in sprint 1*
            if (dbUser != null)
                throw new ValidationException("User with email " + model.Email + " is already registered");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            User user = new User() { Email = model.Email, Name = model.Name };

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            bool success = _userRepository.Insert(user);

            if (!success)
                throw new ValidationException("User not created");

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
                throw new ValidationException("Operation forbidden.");
            }
            if (user == null)
                throw new ValidationException("User not found");

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
                    throw new ValidationException("Old password cannot be empty");
                }
                
                if (userParam.NewPassword.Equals(userParam.NewPasswordCheck)) {
                    
                    if(!VerifyPasswordHash(userParam.OldPassword, user.PasswordHash, user.PasswordSalt))
                    {
                        throw new ValidationException("Wrong password");
                    }
                    
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(userParam.NewPassword, out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }
                else
                {
                    throw new ValidationException("Passwords are not matching");
                }
            }

            var result = _userRepository.Update(user);
            if (!result)
            {
                throw new ValidationException("User non-existent or not altered.");
            }
            else return _userRepository.GetByEmail(user.Email);
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
                throw new ValidationException("User non-existent or not deleted.");
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
            if (password == null) throw new ArgumentNullException("password", "Password cannot be null");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("The password cannot be empty or whitespace only.", "password");

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
            if (password == null) throw new ArgumentNullException("password", "Password cannot be null");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

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
        #endregion
    }
}
