using System;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.AspNetCore.Http;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    // interface for a User service
    public interface IUserService
    {
        UserModel Login(AuthenticateModel model);
        UserModel GetById(Guid id);
        UserModel GetByEmail(String email);
        UserModel GetFromHttpContext(HttpContext httpContext);
        UserModel Register(RegisterModel model);
        void Update(UserModel user, string password = null);
        bool Delete(Guid id);
    }

    // actual implementation of a user service that implements the interface with all the logic

    public class UserService : IUserService
    {
        // getting an instance of a user repository to be able to communicate with the db layer
        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        // logic of a login function, model is passed, user is then looked up in the db through repository
        // if credentials are valid, UserModel is returned
        public UserModel Login(AuthenticateModel model)
        {
            var email = model.Email;
            var password = model.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new Exception("Email or password can't be empty");

            var user = _userRepository.GetByEmail(email);

            // check if username exists
            if (user == null)
                throw new Exception("User with this email does not exist");

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                throw new Exception("Incorrect password");

            // authentication successful, return user

            return new UserModel() {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }
        // getting the user from the db by user's ID
        public UserModel GetById(Guid id)
        {
            var user = _userRepository.Get(id);
            return new UserModel() {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }
        // getting the user from the db by extracting the user information from a http response

        public UserModel GetFromHttpContext(HttpContext httpContext)
        {
            var userId = new Guid(httpContext.User.Identity.Name);

            var user = GetById(userId);

            return user;
        }

        // logic of a registration function with all the necessary validation, creating a pw hash and inserting
        // the newly registered used into the db through repository, returning a UserModel

        public UserModel Register(RegisterModel model)
        {
            // validation
            if (string.IsNullOrWhiteSpace(model.Password))
                throw new Exception("Password is required");

            var dbUser = _userRepository.GetByEmail(model.Email);

            if (dbUser != null)
                throw new Exception("User with email " + model.Email + " is already registered");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            User user = new User() { Email = model.Email, Name = model.Name };

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            bool success = _userRepository.Insert(user);

            if (!success)
                throw new Exception("User not created");

            return new UserModel() { 
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }

        // method to update the information about the user that already exists

        public void Update(UserModel userParam, string password = null)
        {
            /*var user = _context.Users.Find(userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
            {
                // throw error if the new username is already taken
                if (_context.Users.Any(x => x.Username == userParam.Username))
                    throw new AppException("Username " + userParam.Username + " is already taken");

                user.Username = userParam.Username;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
                user.FirstName = userParam.FirstName;

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
                user.LastName = userParam.LastName;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();*/
        }
        
        // method to completely delete a registered user from the database
        public bool Delete(Guid id)
        {
            return _userRepository.Delete(id);
        }

        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
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

        public UserModel GetByEmail(string email)
        {
            var user = _userRepository.GetByEmail(email);
            return new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }
    }
}
