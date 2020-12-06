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
    // adding a layer of abstraction by creating an interface first
    public interface IUserRepository
    {
        public User Get(Guid id);

        public bool Insert(User user);
        public bool Update(User user);

        public User GetByEmail(string email);
        public bool Delete(Guid id);
        public IEnumerable<User> GetAll();
    }

    // actual implementation of a user repository that accesses the database and makes changes to it

    public class UserRepository : IUserRepository
    {
        string connectionString;

        public UserRepository(IConfiguration configuration)
        {
             connectionString = configuration.GetConnectionString("DBConnection");
        }

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

        // logic for getting the model of a user by his ID from the database
        // establishing an SQL connection with the db, querying the db, returning the user model
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
                        PasswordSalt = Convert.FromBase64String(result.PasswordSalt),
                    };

                    return user;
                } 
                catch (Exception e)
                {

                }
            }

            return null;
        }

        // logic for getting the model of a user by his email from the database
        // establishing an SQL connection with the db, querying the db, returning the user model

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
                        PasswordSalt = Convert.FromBase64String(result.PasswordSalt),
                    };

                    return user;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        // inserting newly registered user into the database, taking user model as a parameter
        // sql insert query is populated with data from user and executed, returning success

        public bool Insert(User user)
        {
            string query = "INSERT INTO Users (Id, Email, Name, PasswordHash, PasswordSalt)" +
                " VALUES (@Id, @Email, @Name, @PasswordHash, @PasswordSalt)";

            using (var connection = new SqlConnection(connectionString))
            {
                user.Id = Guid.NewGuid();

                var parameters = new {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    PasswordHash = Convert.ToBase64String(user.PasswordHash),
                    PasswordSalt = Convert.ToBase64String(user.PasswordSalt)
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
