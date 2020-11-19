﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.Entities;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace Group3.Semester3.WebApp.Repositories
{
    // adding a layer of abstraction by creating an interface first
    public interface IUserRepository
    {
        public User Get(int id);

        public bool Insert(User user);

        public User GetByEmail(string email);
    }

    // actual implementation of a user repository that accesses the database and makes changes to it

    public class UserRepository : IUserRepository
    {
        string connectionString;

        public UserRepository(IConfiguration configuration)
        {
             connectionString = configuration.GetConnectionString("DBConnection");
        }

        // logic for getting the model of a user by his ID from the database
        // establishing an SQL connection with the db, querying the db, returning the user model
        public User Get(int id)
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
                catch (Exception exception)
                {
                    return null;
                }
            }
        }

        // inserting newly registered user into the database, taking user model as a parameter
        // sql insert query is populated with data from user and executed, returning success

        public bool Insert(User user)
        {
            string query = "INSERT INTO Users (Email, Name, PasswordHash, PasswordSalt)" +
                " VALUES (@Email, @Name, @PasswordHash, @PasswordSalt)";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new {
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
