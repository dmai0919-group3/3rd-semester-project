using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Group3.Semester3.WebApp.Entities;
using Microsoft.Extensions.Configuration;

namespace Group3.Semester3.WebApp.Repositories
{
    public interface ICommentRepository
    {
        public bool Insert(Comment comment);
        public IEnumerable<Comment> GetByFileId(Guid fileId);
        public IEnumerable<Comment> GetByFileIdAndParentId(Guid fileId, Guid parentId);
    }
    
    public class CommentRepository : ICommentRepository
    {
        string connectionString;

        public CommentRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DBConnection");
        }

        public bool Insert(Comment comment)
        {
            string query = "INSERT INTO Comments (Id, FileId, ParentId, Text)" +
                   " VALUES (@Id, @FileId, @ParentId, @Text)";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    int rowsChanged = connection.Execute(query, comment);

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

        public IEnumerable<Comment> GetByFileId(Guid fileId)
        {
            string query = "SELECT * FROM Comments WHERE FileId=@FileId AND ParentId=@ParentId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileId = fileId, ParentId = Guid.Empty};

                connection.Open();

                var result = connection.Query<Comment>(query, parameters);

                return result;
            }
        }

        public IEnumerable<Comment> GetByFileIdAndParentId(Guid fileId, Guid parentId)
        {
            string query = "SELECT * FROM Comments WHERE FileId=@FIleId AND ParentId=@ParentId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { FileId = fileId, ParentId = parentId };

                connection.Open();

                var result = connection.Query<Comment>(query, parameters);

                return result;
            }
        }
    }
}