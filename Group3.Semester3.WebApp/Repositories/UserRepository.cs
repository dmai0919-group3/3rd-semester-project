using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.Entities;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using Group3.Semester3.WebApp.Helpers.Exceptions;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their id
        /// </summary>
        /// <param name="id">The Guid of the user</param>
        /// <returns>A User object matching the given id or null if the user doesn't exist.</returns>
        public User Get(Guid id);

        /// <summary>
        /// Adds a new user to the database
        /// </summary>
        /// <param name="user">The User object containing the user's data</param>
        /// <returns>True if the user is saved and false if not.</returns>
        public bool Insert(User user);
        public bool Update(User user);

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>The User object matching the given email address or null if there is no user with the given email address.</returns>
        public User GetByEmail(string email);

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="id">The Guid of the user to be deleted</param>
        /// <returns>True if the delete is successful and false if not.</returns>
        public bool Delete(Guid id);
        public IEnumerable<User> GetAll();
    }

    public class UserRepository : IUserRepository
    {
        string connectionString;

        public UserRepository(IConfiguration configuration)
        {
             connectionString = configuration.GetConnectionString("DBConnection");
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="id">The Guid of the user to be deleted</param>
        /// <returns>True if the delete is successful and false if not.</returns>
        public bool Delete(Guid id)
        {
            string query = "DELETE FROM Users WHERE id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = id };

                try
                {
                    connection.Open();

                    int rowsChanged = connection.Execute(query, parameters);
                    if (rowsChanged > 0)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a user by their id
        /// </summary>
        /// <param name="id">The Guid of the user</param>
        /// <returns>A User object matching the given id or null if the user doesn't exist.</returns>
        public User Get(Guid id)
        {
            string query = "SELECT TOP 1 * FROM Users WHERE id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = id };

                try
                {
                    connection.Open();

                    var result = connection.QueryFirst(query, parameters);

                    User user = new User()
                    {
                        Id = result.Id,
                        Email = result.Email,
                        Name = result.Name,
                        PasswordHash = Convert.FromBase64String(result.PasswordHash),
                        PasswordSalt = Convert.FromBase64String(result.PasswordSalt)
                    };

                    return user;
                } 
                catch (Exception e)
                {

                }
            }

            return null;
        }

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>The User object matching the given email address or null if there is no user with the given email address.</returns>
        public User GetByEmail(string email)
        {

            string query = "SELECT TOP 1 * FROM Users WHERE Email=@Email";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Email = email };

                try
                {
                    connection.Open();

                    var result = connection.QueryFirst(query, parameters);

                    User user = new User()
                    {
                        Id = result.Id,
                        Email = result.Email,
                        Name = result.Name,
                        PasswordHash = Convert.FromBase64String(result.PasswordHash),
                        PasswordSalt = Convert.FromBase64String(result.PasswordSalt)
                    };

                    return user;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds a new user to the database
        /// </summary>
        /// <param name="user">The User object containing the user's data</param>
        /// <returns>True if the user is saved and false if not.</returns>
        public bool Insert(User user)
        {
            string query = "INSERT INTO Users (Id, Email, Name, PasswordHash, PasswordSalt, Activated)" +
                " VALUES (@Id, @Email, @Name, @PasswordHash, @PasswordSalt, @Activated)";

            using (var connection = new SqlConnection(connectionString))
            {
                user.Id = Guid.NewGuid();

                var parameters = new {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    PasswordHash = Convert.ToBase64String(user.PasswordHash),
                    PasswordSalt = Convert.ToBase64String(user.PasswordSalt),
                    Activated = true
                };

                try
                {
                    connection.Open();
                    int rowsChanged = connection.Execute(query, parameters);

                    if (rowsChanged > 0)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                }
            }

            return false;
        }

        public bool Update(User user)
        {
            string query = "UPDATE Users SET Email=@Email, Name=@Name, PasswordHash=@PasswordHash, PasswordSalt=@PasswordSalt WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    Name = user.Name,
                    Id = user.Id,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    PasswordSalt = user.PasswordSalt
                };

                try
                {
                    connection.Open();

                    int rowsChanged = connection.Execute(query, parameters);
                    if (rowsChanged > 0)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                }
                return false;

            }
        }

        public IEnumerable<User> GetAll()
        {
            string query = "SELECT * FROM Users";

            using (var connection = new SqlConnection(connectionString))
            {

                try
                {
                    connection.Open();

                    var result = connection.Query<User>(query);

                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }
    }
}
