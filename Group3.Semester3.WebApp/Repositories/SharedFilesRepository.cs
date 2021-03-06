﻿using Dapper;
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
        public bool DeleteForAll(Guid fileId);
        public bool DeleteBySharedFile(SharedFile sharedFile);
        public bool IsSharedWithUser(Guid fileId, Guid userId);
        public bool IsSharedWithUser(IEnumerable<FileEntity> files, Guid userId);
        public IEnumerable<UserModel> GetUsersByFileId(Guid fileId);
        public string GetHashByFileId(Guid fileId);

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
            
            string query = "SELECT Files.* FROM SharedFiles JOIN Files ON (Files.Id = SharedFiles.FileId) "+
                           "WHERE SharedFiles.UserId = @UserId";

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

                try
                {
                    connection.Open();

                    var result = connection.QuerySingle<FileEntity>(query, parameters);

                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
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
        public bool DeleteForAll(Guid fileId)
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

        public bool IsSharedWithUser(IEnumerable<FileEntity> files, Guid userId)
        {
            string query = "SELECT * FROM SharedFiles WHERE UserId=@UserId AND FileId IN @FileIds";
            
            var fileIds = files.Select(file => file.Id).ToList();

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileIds = fileIds, UserId = userId};

                try
                {
                    connection.Open();

                    var result = connection.Query(query, parameters);
                
                    return result.Any();
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public IEnumerable<UserModel> GetUsersByFileId(Guid fileId)
        {
            string query = "SELECT Users.* FROM Users JOIN SharedFiles ON Users.Id = SharedFiles.UserId WHERE SharedFiles.FileId = @FileId";

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
                    return new List<UserModel>();
                }
            }
        }

        public string GetHashByFileId(Guid fileId)
        {
            string query = "SELECT * FROM SharedFilesLinks WHERE FileId=@FileId";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var result = connection.QuerySingle(query, new {FileId = fileId});
                    
                    return result.Hash;
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        public bool DeleteBySharedFile(SharedFile sharedFile)
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
            string query = "INSERT INTO SharedFilesLinks (FileId, Hash)" +
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
