﻿using System;
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
        UserModel Login(AuthenticateModel model);
        UserModel GetById(Guid id);
        UserModel GetByEmail(String email);
        UserModel GetFromHttpContext(HttpContext httpContext);
        UserModel Register(RegisterModel model);
        User Update(User user, string password = null);
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
                throw new ValidationException("Email or password cannot be empty.");

            var user = _userRepository.GetByEmail(email);

            // check if username exists
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                throw new ValidationException("Incorrect email or password.");

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
            if(user != null) {
            return new UserModel() {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
            }else throw new ValidationException("No user found.");
        }
        // getting the user from the db by extracting the user information from a http response

        public UserModel GetFromHttpContext(HttpContext httpContext)
        {
            var userId = new Guid(httpContext.User.Identity.Name);

            var user = GetById(userId);
            if(user == null)
            {
                throw new ValidationException("User not found");
            }
            else return user;
        }

        // logic of a registration function with all the necessary validation, creating a pw hash and inserting
        // the newly registered used into the db through repository, returning a UserModel

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

            return new UserModel() { 
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }

        // method to update the information about the user that already exists

        public User Update(User userParam, string password = null)
        {
            var user = _userRepository.GetByEmail(userParam.Email);

            if (user == null)
                throw new ValidationException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Name) && userParam.Name != user.Name)
            {
                // throw error if the new username is already taken
                if (_userRepository.GetAll().Any(x => x.Name == userParam.Name))
                    throw new ValidationException("Username " + userParam.Name + " is already taken");

                user.Name = userParam.Name;
            }

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            var result = _userRepository.Update(user);
            if (!result)
            {
                throw new ValidationException("User non-existent or not altered.");
            }
            else return _userRepository.GetByEmail(user.Email);
        }
        
        // method to completely delete a registered user from the database
        public bool Delete(Guid id)
        {

            bool result =_userRepository.Delete(id);
            if (!result)
            {
                throw new ValidationException("User non-existent or not deleted.");
            }
            else return result;
            
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
            try
            {
                var user = _userRepository.GetByEmail(email);
                return new UserModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name
                };
            } catch(Exception e)
            {
                return null;
            }
        }
    }
}
