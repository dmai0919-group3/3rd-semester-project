using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.Entities;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface IUserRepository
    {
        public User Get(Guid id);

        public bool Insert(User user);

        public User GetByEmail(string email);
        public bool Delete(Guid id);
    }

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
                catch (Exception exception)
                {

                }
            }

            return null;
        }

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
                catch (Exception exception)
                {
                    return null;
                }
            }
        }

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
                catch (Exception exception)
                {
                    throw exception;
                }
            }

            return false;
        }
    }
}
