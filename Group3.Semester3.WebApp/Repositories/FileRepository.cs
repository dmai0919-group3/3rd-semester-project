using Dapper;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface IFileRepository
    {
        public FileEntity GetById(Guid id);
        public IEnumerable<FileEntity> GetByUserId(Guid userId);
        public IEnumerable<FileEntity> GetByParentId(Guid parentId);
        public IEnumerable<FileEntity> GetByUserIdAndParentId(Guid userId, Guid parentId);
        public bool Insert(FileEntity fileEntity);
        public bool Delete(Guid id);
        public bool Rename(Guid id, string name);
        public bool MoveIntofolder(Guid fileId, Guid parentId);
    }
    public class FileRepository : IFileRepository
    {
        string connectionString;

        public FileRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DBConnection");
        }
        public FileEntity GetById(Guid id)
        {
            string query = "SELECT TOP 1 * FROM Files WHERE id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = id };

                try
                {
                    connection.Open();

                    var result = connection.QueryFirst<FileEntity>(query, parameters);
                    
                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }

        public IEnumerable<FileEntity> GetByUserId(Guid userId)
        {
            string query = "SELECT * FROM Files WHERE userId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { UserId = userId };

                try
                {
                    connection.Open();

                    var result = connection.Query<FileEntity>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }

        public bool Insert(FileEntity fileEntity)
        {
            string query = "INSERT INTO Files (Id, UserId, AzureName, Name, ParentId, IsFolder, Updated)" +
                   " VALUES (@Id, @UserId, @AzureName, @Name, @ParentId, @IsFolder, @Updated)";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    int rowsChanged = connection.Execute(query, fileEntity);

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

        public bool Delete(Guid id)
        {
            string query = "DELETE FROM Files WHERE Id=@Id";

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
                }
                return false;
            }
        }

        public bool Rename(Guid id, string name)
        {
            string query = "UPDATE Files SET Name=@Name, Updated=@Updated WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    Name = name,
                    Id = id,
                    Updated = DateTime.Now
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

        public IEnumerable<FileEntity> GetByParentId(Guid parentId)
        {
            string query = "SELECT * FROM Files WHERE ParentId=@ParentId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { ParentId = parentId };

                try
                {
                    connection.Open();

                    var result = connection.Query<FileEntity>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }

        public IEnumerable<FileEntity> GetByUserIdAndParentId(Guid userId, Guid parentId)
        {
            string query = "SELECT * FROM Files WHERE UserId=@UserId AND ParentId=@ParentId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { 
                    UserId = userId,
                    ParentId = parentId
                };

                try
                {
                    connection.Open();

                    var result = connection.Query<FileEntity>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }

        public bool MoveIntofolder(Guid fileId, Guid parentId)
        {
            string query = "UPDATE Files SET ParentId=@ParentId WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    ParentId = parentId,
                    Id = fileId
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
    }
}
