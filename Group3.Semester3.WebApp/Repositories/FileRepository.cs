using Dapper;
using Group3.Semester3.WebApp.Entities;
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
        public File GetById(Guid id);
        public List<File> GetByUserId(Guid userId);

        public bool Insert(File file);
        public bool Delete(Guid id);
    }
    public class FileRepository : IFileRepository

    {
        string connectionString;

        public FileRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DBConnection");
        }
        public File GetById(Guid id)
        {
            string query = "SELECT TOP 1 * FROM Files WHERE id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { Id = id };

                try
                {
                    connection.Open();

                    var result = connection.QueryFirst(query, parameters);

                    File file = new File()
                    {
                        Id = result.Id,
                        UserId = result.UserId,
                        AzureId = result.AzureId,
                        Name = result.Name
                    };

                    return file;
                }
                catch (Exception exception)
                {

                }
            }

            return null;
        }

        public List<File> GetByUserId(Guid userId)
        {
            string query = "SELECT * FROM Files WHERE userId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new { UserId = userId };

                try
                {
                    connection.Open();

                    var result = connection.QueryFirst<List<File>>(query, parameters);

                    return result;
                }
                catch (Exception exception)
                {

                }
            }

            return null;
        }

        public bool Insert(File file)
        {
            string query = "INSERT INTO Files (Id, UserId, AzureId, Name)" +
                   " VALUES (@Id, @UserId, @AzureId, @Name)";

            using (var connection = new SqlConnection(connectionString))
            {

                try
                {
                    connection.Open();
                    int rowsChanged = connection.Execute(query, file);

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

        public bool Delete(Guid id)
        {
            string query = "DELETE FROM Files WHERE id=@Id";

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
    }
}
