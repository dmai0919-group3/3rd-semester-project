using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Models.FileSystem;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.AspNetCore.Http;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IFileService
    {
        public Task<List<FileEntry>> UploadFile(UserModel user, string parentGUID, List<IFormFile> files);
    }
    public class FileService : IFileService
    {
        private IConfiguration _configuration;
        private IFileRepository _fileRepository;

        public FileService(IConfiguration configuration, IFileRepository fileRepository)
        {
            _configuration = configuration;
            _fileRepository = fileRepository;
        }

        public async Task<List<FileEntry>> UploadFile(UserModel user, string parentGUID, List<IFormFile> files)
        {
            //long size = files.Sum(f => f.Length);

            var parsedGUID = ParseGuid(parentGUID);

            List<FileEntry> fileEntries = new List<FileEntry>();

            BlobContainerClient containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            containerClient.CreateIfNotExists();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var blobName = Guid.NewGuid();
                    try
                    {
                        await containerClient.UploadBlobAsync(blobName.ToString(), formFile.OpenReadStream());
                        fileEntries.Add(new FileEntry
                        {
                            Name = formFile.FileName,
                            UUID = blobName,
                            Parent = new DirectoryEntry { UUID = parsedGUID }
                        });
                        
                        var file = new FileEntity()
                        {
                            Id = Guid.NewGuid(),
                            AzureId = blobName,
                            Name = formFile.FileName,
                            UserId = user.Id,
                        };
                        
                        _fileRepository.Insert(file);
                    }
                    catch
                    {
                        // TODO generate error
                    }
                }
            }

            // TODO push entries to db

            return fileEntries;
        }

        private Guid ParseGuid(string guid)
        {
            Guid parsedGuid = Guid.Empty;

            try
            {
                parsedGuid = System.Guid.Parse(guid);
            }
            catch { }

            return parsedGuid;
        }
    }
}