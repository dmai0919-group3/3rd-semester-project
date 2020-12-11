using Dapper;
using Group3.Semester3.WebApp.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.Models.Groups;
using Group3.Semester3.WebApp.Models.Users;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface IGroupRepository
    {
        public bool Insert(Group group);
        public bool Rename(Guid groupId, string name);
        public bool Delete(Guid groupId);
        public IEnumerable<Group> GetByUserId(Guid userId);
        public IEnumerable<UserModel> GetUsersByGroupId(Guid groupId);
        public Group GetByGroupId(Guid groupId);
        public bool AddUser(UserGroupModel model);
        public bool RemoveUser(Guid groupId, Guid userId);
        public bool IsUserInGroup(Guid groupId, Guid userId);
        public UserGroupModel GetUserGroupModel(Guid groupId, Guid userId);
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
            string query = "SELECT Groups.* FROM Groups JOIN UsersGroups ON Groups.Id=UsersGroups.GroupId WHERE UsersGroups.UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { UserId = userId };

                try
                {
                    connection.Open();

                    var result = connection.Query<Group>(query, parameters);

                    return result;
                }
                catch
                {
                    return new List<Group>();
                }
            }
        }

        public IEnumerable<UserModel> GetUsersByGroupId(Guid groupId)
        {
            string query = "SELECT Users.*, UsersGroups.Permissions as PermissionsNumber FROM Users JOIN UsersGroups ON Users.Id=UsersGroups.UserId WHERE UsersGroups.GroupId=@GroupId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { GroupId = groupId };

                try
                {
                    connection.Open();

                    var result = connection.Query<UserModel>(query, parameters);

                    return result;
                }
                catch
                {
                    return new List<UserModel>();
                }
            }
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
                catch
                {
                    // ignored
                }

                return false;
            }
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

                    var group = connection.QuerySingle<Group>(query, parameters);

                    return group;
                }
                catch (Exception e)
                {
                    return null;
                }
            }

        }

        public bool AddUser(UserGroupModel model)
        {
            string query = "INSERT INTO UsersGroups (UserId, GroupId, Permissions)" +
                   " VALUES (@UserId, @GroupId, @PermissionsNumber)";
            
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    int rowsChanged = connection.Execute(query, model);

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

        public bool RemoveUser(Guid groupId, Guid userId)
        {
            string query = "DELETE FROM UsersGroups WHERE GroupId=@GroupId AND UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { GroupId = groupId, UserId = userId };

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

        public bool IsUserInGroup(Guid groupId, Guid userId)
        {
            string query = "SELECT * FROM UsersGroups WHERE GroupId=@GroupId AND UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { GroupId = groupId, UserId = userId };
                
                connection.Open();

                var result = connection.Query(query, parameters);

                return result.Any();
            }
        }

        public UserGroupModel GetUserGroupModel(Guid groupId, Guid userId)
        {
            string query = "SELECT Permissions FROM UsersGroups WHERE GroupId=@GroupId AND UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { GroupId = groupId, UserId = userId };
                
                connection.Open();

                var result = connection.QuerySingle<UserGroupModel>(query, parameters);

                return result;
            }
        }
    }
}
