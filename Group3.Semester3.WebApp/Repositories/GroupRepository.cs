﻿using Dapper;
using Group3.Semester3.WebApp.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface IGroupRepository
    {
        public bool Insert(Group group);
        public bool Rename(Guid groupId, string name);
        public bool Delete(Guid groupId);
        public IEnumerable<Group> GetByUserId(Guid userId);
        public Group GetByGroupId(Guid groupId);
    }

    public class GroupRepository : IGroupRepository
    {
        string connectionString;

        public GroupRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DBConnection");
        }

        public IEnumerable<Group> GetByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }

        public bool Insert(Group group)
        {
            string query = "INSERT INTO Groups (Id, Name)" +
                   " VALUES (@Id, @Name)";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    int rowsChanged = connection.Execute(query, group);

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

        public bool Rename(Guid groupId, string name)
        {
            string query = "UPDATE Groups SET Name=@Name WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    Name = name,
                    Id = groupId,
                    
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
        public bool Delete(Guid groupId)
        {
            string query = "DELETE FROM Groups WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = groupId };

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

        public Group GetByGroupId(Guid groupId)
        {
            string query = "SELECT TOP 1 * FROM Groups WHERE id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = groupId };

                try
                {
                    connection.Open();

                    var result = connection.QueryFirst(query, parameters);

                    Group group = new Group()
                    {
                        Id = result.Id,
                        Name = result.Name,
                    };

                    return group;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }
    }
}