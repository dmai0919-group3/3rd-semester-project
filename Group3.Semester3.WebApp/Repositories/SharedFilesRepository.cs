using Dapper;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Models.FileSystem;
using Group3.Semester3.WebApp.Models.Users;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface ISharedFilesRepository
    {
        public IEnumerable<FileEntity> GetByUserId(Guid userId);
        public FileEntity GetByLink(string hash);
        public bool Insert(SharedFile sharedFile);
        public bool InsertWithLink(SharedFileLink sharedFileLink);
        public bool DeleteShareLinkByFileId(Guid fileId);
        public bool DeleteByFileIdFromSharedForAll(Guid fileId);
        public bool DeleteByFileIdFromSharedForOne(SharedFile sharedFile);
        public bool IsSharedWithUser(Guid fileId, Guid userId);
        public IEnumerable<UserModel> GetUsersByFileId(Guid fileId);

    }
    public class SharedFilesRepository : ISharedFilesRepository
    {
        string connectionString;

        public SharedFilesRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DBConnection");
        }


        public IEnumerable<FileEntity> GetByUserId(Guid userId)
        {
            
            string query = "SELECT Files.* FROM SharedFiles JOIN Files ON (Files.Id = SharedFiles.FileId) WHERE SharedFiles.UserId = @UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { UserId = userId };

                connection.Open();

                var result = connection.Query<FileEntity>(query, parameters);

                return result;
            }
        }

        public FileEntity GetByLink(string hash)
        {
            string query = "SELECT Files.* FROM SharedFilesLinks JOIN Files ON (Files.Id = SharedFilesLinks.FileId) WHERE SharedFilesLinks.Hash = @Hash";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Hash = hash };

                connection.Open();

                var result = connection.QueryFirst<FileEntity>(query, parameters);

                return result;
            }
        }

        public bool Insert(SharedFile sharedFile)
        {
            string query = "INSERT INTO SharedFiles (FileId, UserId)" +
                   " VALUES (@FileId, @UserId)";

            using (var connection = new SqlConnection(connectionString))
            {
               
                connection.Open();
                int rowsChanged = connection.Execute(query, sharedFile);

                if (rowsChanged > 0)
                {
                    return true;
                }
            }

            return false;
        }
        public bool DeleteByFileIdFromSharedForAll(Guid fileId)
        {
            string query = "DELETE FROM SharedFiles WHERE FileId=@FileId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileId = fileId };

                connection.Open();

                int rowsChanged = connection.Execute(query, parameters);
                if (rowsChanged > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsSharedWithUser(Guid fileId, Guid userId)
        {
            var query = "SELECT * FROM SharedFiles WHERE FileId=@FileId AND UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileId = fileId, UserId = userId};

                connection.Open();

                var result = connection.Query(query, parameters);
                
                return result.Any();
            }
        }

        public IEnumerable<UserModel> GetUsersByFileId(Guid fileId)
        {
            string query = "SELECT Users.*FROM Users JOIN SharedFiles ON Users.Id = SharedFiles.FileId WHERE SharedFiles.FileId = @FileId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileId = fileId };

                try
                {
                    connection.Open();

                    var result = connection.Query<UserModel>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {
                }
                throw new NotImplementedException();
            }
        }

        public bool DeleteByFileIdFromSharedForOne(SharedFile sharedFile)
        {
            string query = "DELETE FROM SharedFiles WHERE FileId=@FileId AND UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();

                int rowsChanged = connection.Execute(query, sharedFile);
                if (rowsChanged > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool InsertWithLink(SharedFileLink sharedFileLink)
        {
            string query = "INSERT INTO SharedFilesLinkS (FileId, Hash)" +
                   " VALUES (@FileId, @Hash)";

            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();
                int rowsChanged = connection.Execute(query, sharedFileLink);

                if (rowsChanged > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool DeleteShareLinkByFileId(Guid fileId)
        {
            string query = "DELETE FROM SharedFilesLinks WHERE FileId=@FileId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileId = fileId };

                connection.Open();

                int rowsChanged = connection.Execute(query, parameters);
                if (rowsChanged > 0)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
