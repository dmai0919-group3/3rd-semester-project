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
        /// <summary>
        /// Gets a file by it's id
        /// </summary>
        /// <param name="id">The Guid of a file</param>
        /// <returns>The FileEntity matching the given ID</returns>
        public FileEntity GetById(Guid id);

        /// <summary>
        /// Gets all files owned by a given user
        /// </summary>
        /// <param name="userId">The Guid of the user</param>
        /// <returns>An IEnumerable containing all the FileEntities owned by the given user</returns>
        public IEnumerable<FileEntity> GetByUserId(Guid userId);

        /// <summary>
        /// Gets all files that has a given parent id
        /// </summary>
        /// <param name="parentId">The Guid of the parent folder</param>
        /// <returns>An IEnumerable containing all FileEntities that match the query</returns>
        /// 
        public IEnumerable<FileEntity> GetByParentId(Guid parentId);
        public IEnumerable<FileEntity> GetFoldersByParentId(Guid parentId);
        public IEnumerable<FileEntity> GetByGroupId(Guid groupId);

        /// <summary>
        /// Gets all files owned by a given user and inside a given parent folder
        /// </summary>
        /// <param name="userId">The Guid of the owner user</param>
        /// <param name="parentId">The Guid of the parent folder</param>
        /// <returns>An IEnumerable containing all the FileEntities matching the query</returns>
        public IEnumerable<FileEntity> GetByUserIdAndParentId(Guid userId, Guid parentId);
        public IEnumerable<FileEntity> GetByGroupIdAndParentId(Guid groupId, Guid parentId);

        /// <summary>
        /// Adds a new file to the database
        /// </summary>
        /// <param name="fileEntity">The FileEntity to be added</param>
        /// <returns>True if the file has been added false if not</returns>
        public bool Insert(FileEntity fileEntity);

        /// <summary>
        /// Deletes a file from the database
        /// </summary>
        /// <param name="id">The Guid of the file to be deleted</param>
        /// <returns>True if the file has been deleted false if not.</returns>
        public bool Delete(Guid id);

        /// <summary>
        /// Updates a file in the database
        /// </summary>
        /// <param name="fileEntity">A file</param>
        /// <returns>True if the file has been updated, false if not.</returns>
        public bool Update(FileEntity fileEntity);

        /// <summary>
        /// Moves a file to a new folder (changes the parentId of the file)
        /// </summary>
        /// <param name="fileId">The Guid of the file</param>
        /// <param name="parentId">The Guid of the new parent folder</param>
        /// <returns>True if the parent has changed and false if not.</returns>
        public bool MoveIntofolder(Guid fileId, Guid parentId);

        public bool InsertFileVersion(FileVersion fileVersion);
        
        public IEnumerable<FileVersion> GetFileVersions(Guid fileId);

        public FileEntity GetFile(Guid parentId, Guid userId, Guid groupId, string name);

        public FileVersion GetFileVersion(Guid id);

        public IEnumerable<FileEntity> GetParents(Guid id);

    }
    public class FileRepository : IFileRepository
    {
        string connectionString;

        public FileRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DBConnection");
        }
        
        /// <summary>
        /// Gets a file by it's id
        /// </summary>
        /// <param name="id">The Guid of a file</param>
        /// <returns>The FileEntity matching the given ID</returns>
        public FileEntity GetById(Guid id)
        {
            string query = "SELECT TOP 1 * FROM Files WHERE Id=@Id";

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

        /// <summary>
        /// Gets all files owned by a given user
        /// </summary>
        /// <param name="userId">The Guid of the user</param>
        /// <returns>An IEnumerable containing all the FileEntities owned by the given user</returns>
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

        public IEnumerable<FileEntity> GetFoldersByParentId(Guid parentId)
        {
            string query = "SELECT * FROM Files WHERE ParentId=@ParentId and IsFolder='1'";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { ParentId = parentId };

                connection.Open();

                var result = connection.Query<FileEntity>(query, parameters);

                return result;
            }
        }

        public IEnumerable<FileEntity> GetByGroupId(Guid groupId)
        {
            string query = "SELECT * FROM Files WHERE groupId=@GroupId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { GroupId = groupId };

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

        /// <summary>
        /// Adds a new file to the database
        /// </summary>
        /// <param name="fileEntity">The FileEntity to be added</param>
        /// <returns>True if the file has been added false if not</returns>
        public bool Insert(FileEntity fileEntity)
        {
            string query = "INSERT INTO Files (Id, UserId, GroupId, AzureName, Name, ParentId, IsFolder, Updated, Size)" +
                   " VALUES (@Id, @UserId, @GroupId, @AzureName, @Name, @ParentId, @IsFolder, @Updated, @Size)";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                int rowsChanged = connection.Execute(query, fileEntity);

                if (rowsChanged > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes a file from the database
        /// </summary>
        /// <param name="id">The Guid of the file to be deleted</param>
        /// <returns>True if the file has been deleted false if not.</returns>
        public bool Delete(Guid id)
        {
            string query = "DELETE FROM Files WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = id };
                
                connection.Open();

                int rowsChanged = connection.Execute(query, parameters);
                if (rowsChanged > 0)
                {
                    return true;
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// Updates a file in the database
        /// </summary>
        /// <param name="fileEntity">A file</param>
        /// <returns>True if the file has been updated, false if not.</returns>
        public bool Update(FileEntity fileEntity)
        {
            string query = "UPDATE Files SET Name=@Name, Updated=@Updated, IsShared=@IsShared, Size=@Size, AzureName=@AzureName WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int rowsChanged = connection.Execute(query, fileEntity);
                if (rowsChanged > 0)
                {
                    return true;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Gets all files that has a given parent id
        /// </summary>
        /// <param name="parentId">The Guid of the parent folder</param>
        /// <returns>An IEnumerable containing all FileEntities that match the query</returns>
        public IEnumerable<FileEntity> GetByParentId(Guid parentId)
        {
            string query = "SELECT * FROM Files WHERE ParentId=@ParentId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { ParentId = parentId };

                connection.Open();

                var result = connection.Query<FileEntity>(query, parameters);

                return result;
            }
        }

        /// <summary>
        /// Gets all files owned by a given user and inside a given parent folder
        /// </summary>
        /// <param name="userId">The Guid of the owner user</param>
        /// <param name="parentId">The Guid of the parent folder</param>
        /// <returns>An IEnumerable containing all the FileEntities matching the query</returns>
        public IEnumerable<FileEntity> GetByUserIdAndParentId(Guid userId, Guid parentId)
        {
            string query = "SELECT * FROM Files WHERE UserId=@UserId AND ParentId=@ParentId AND GroupId=@GroupId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { 
                    UserId = userId,
                    ParentId = parentId,
                    GroupId = Guid.Empty
                };

                connection.Open();

                var result = connection.Query<FileEntity>(query, parameters);

                return result;
            }
        }

        public IEnumerable<FileEntity> GetByGroupIdAndParentId(Guid groupId, Guid parentId)
        {
            string query = "SELECT * FROM Files WHERE GroupId=@GroupId AND ParentId=@ParentId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    GroupId = groupId,
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

        /// <summary>
        /// Moves a file to a new folder (changes the parentId of the file)
        /// </summary>
        /// <param name="fileId">The Guid of the file</param>
        /// <param name="parentId">The Guid of the new parent folder</param>
        /// <returns>True if the parent has changed and false if not.</returns>
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

        public bool InsertFileVersion(FileVersion fileVersion)
        {
            string query = "INSERT INTO FileVersions (Id, FileId, AzureName, Note, Created, UserId)" +
                           " VALUES (@Id, @FileId, @AzureName, @Note, @Created, @UserId)";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    
                    int rowsChanged = connection.Execute(query, fileVersion);

                    if (rowsChanged > 0)
                    {
                        return true;
                    }
                }
                catch
                {
                    
                }
                
            }

            return false;
        }

        public IEnumerable<FileVersion> GetFileVersions(Guid fileId)
        {
            string query = "SELECT FileVersions.*, Users.Name AS Username FROM FileVersions " +
            "JOIN Users ON FileVersions.UserId=Users.Id WHERE FileId=@FileId ORDER BY Created DESC";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    FileId = fileId
                };

                try
                {
                    connection.Open();

                    var result = connection.Query<FileVersion>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {
                    var s = "";
                }
            }

            return new List<FileVersion>();
        }

        public FileEntity GetFile(Guid parentId, Guid userId, Guid groupId, string name)
        {
            string query = "SELECT * FROM Files WHERE GroupId=@GroupId AND ParentId=@ParentId AND UserId=@UserId AND Name=@Name";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    GroupId = groupId,
                    UserId = userId,
                    ParentId = parentId,
                    Name = name
                };

                try
                {
                    connection.Open();

                    var result = connection.QuerySingle<FileEntity>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }

        public FileVersion GetFileVersion(Guid id)
        {
            string query = "SELECT * FROM FileVersions WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    Id = id
                };

                try
                {
                    connection.Open();

                    var result = connection.QuerySingle<FileVersion>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {

                }
            }

            return null;
        }

        public IEnumerable<FileEntity> GetParents(Guid id)
        {
            string query = ";WITH CTE AS " +
                "(SELECT a.Id, a.ParentId, a.Name, a.GroupId FROM Files a WHERE Id = @Id " +
            "UNION ALL SELECT a.Id, a.ParentId, a.Name, a.GroupId FROM Files a JOIN cte c ON c.ParentId = a.Id ) " +
            "SELECT *  FROM CTE";
            
            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    Id = id
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
    }
}
