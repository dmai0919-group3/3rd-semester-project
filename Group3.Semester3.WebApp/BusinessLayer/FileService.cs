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
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Azure.Storage.Sas;
using Azure.Storage;
using Azure.Storage.Blobs.Specialized;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IFileService
    {
        public Task<List<FileEntry>> UploadFile(UserModel user, string parentGUID, List<IFormFile> files);
        public IEnumerable<FileEntity> BrowseFiles(UserModel currentUser, string parentId);
        public FileEntity RenameFile(Guid id, Guid userId, string name);
        public FileEntity GetById(Guid id);
        public bool DeleteFile(Guid fileId, Guid userId);
        public FileEntity CreateFolder(UserModel user, CreateFolderModel model);
        public bool MoveIntoFolder(FileEntity model, Guid userId);
        public (FileEntity, string) DownloadFile(Guid fileId, Guid userId);
    }
    public class FileService : IFileService
    {
        private IConfiguration _configuration;
        private IFileRepository _fileRepository;

        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public FileService(IConfiguration configuration, IFileRepository fileRepository)
        {
            _configuration = configuration;
            _fileRepository = fileRepository;
        }

        public IEnumerable<FileEntity> BrowseFiles(UserModel currentUser, string parentId)
        {
            var parentGuid = ParseGuid(parentId);

            var fileList = _fileRepository.GetByUserIdAndParentId(currentUser.Id, parentGuid);
                        
            return fileList;
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
                    var blobGuid = Guid.NewGuid();
                    try
                    {
                        await containerClient.UploadBlobAsync(blobGuid.ToString(), formFile.OpenReadStream());
                        fileEntries.Add(new FileEntry
                        {
                            Name = formFile.FileName,
                            Id = blobGuid,
                            Parent = new DirectoryEntry { Id = parsedGUID }
                        });

                        var file = new FileEntity()
                        {
                            Id = Guid.NewGuid(),
                            AzureName = blobGuid.ToString(),
                            Name = formFile.FileName,
                            UserId = user.Id,
                            ParentId = parsedGUID,
                            IsFolder = false
                        };

                        _fileRepository.Insert(file);
                    }
                    catch (Exception e)
                    {
                        // TODO generate error
                    }
                }
                else throw new ValidationException("No files chosen.");
            }

            // TODO push entries to db

            return fileEntries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userId"></param>
        /// <returns>A tuple with the FileEntity with the given fileId and a string with the download URL</returns>
        public (FileEntity, string) DownloadFile(Guid fileId, Guid userId)
        {
            var file = _fileRepository.GetById(fileId);

            if (userId == file.UserId)
            {
                BlobContainerClient containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

                containerClient.CreateIfNotExists();

                BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
                {
                    StartsOn = DateTime.UtcNow,
                    ExpiresOn = DateTime.UtcNow.AddHours(24),
                    BlobContainerName = containerClient.Name,
                    BlobName = fileId.ToString(),
                    Resource = "b"
                };

                blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                StorageSharedKeyCredential storageSharedKeyCredential = new StorageSharedKeyCredential(
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureStorageAccount,
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureAccountKey);

                string sasToken = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();

                return (file, $"{containerClient.GetBlockBlobClient(fileId.ToString()).Uri}?{sasToken}");
            }
            else throw new ValidationException("Operation forbidden.");
        }

        public bool DeleteFile(Guid fileId, Guid userId)
        {
            BlobContainerClient containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            var file = _fileRepository.GetById(fileId);
            if (userId == file.UserId)
            {
                // TODO AzureId should be string
                containerClient.DeleteBlob(_fileRepository.GetById(fileId).AzureName.ToString());
                var result = _fileRepository.Delete(fileId);

                if (!result)
                {
                    throw new ValidationException("File non-existent or not deleted.");
                }
                else return result;
            }
            else throw new ValidationException("Operation forbidden.");
        }

        public FileEntity RenameFile(Guid fileId, Guid userId, string name)
        {
            var file = _fileRepository.GetById(fileId);
            if (userId == file.UserId)
            {
                var result = _fileRepository.Rename(fileId, name);
                if (!result)
                {
                    throw new ValidationException("File non-existent or not deleted.");
                }
                else return _fileRepository.GetById(fileId);
            }
            else throw new ValidationException("Operation forbidden.");
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

        public FileEntity GetById(Guid id)
        {
            var file = _fileRepository.GetById(id);
            if (file == null)
            {
                throw new ValidationException("No file found.");
            }
            else return file;
        }

        public FileEntity CreateFolder(UserModel user, CreateFolderModel model)
        {
            
            var parentGuid = ParseGuid(model.ParentId);

            // Check if user owns parent folder
            if (!parentGuid.Equals(Guid.Empty))
            {
                var parent = GetById(parentGuid);
                
                // TODO: Check for other permissions in the future
                if (parent.UserId != user.Id)
                {
                    throw new ValidationException("Operation fobidden");
                }
            }

            var folder = new FileEntity()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                AzureName = string.Empty,
                UserId = user.Id,
                ParentId = parentGuid,
                IsFolder = true
            };

            var created = _fileRepository.Insert(folder);

            if (!created)
            {
                throw new Exception("Failed to create folder");
            }            
            

            return folder;
        }

        public bool MoveIntoFolder(FileEntity model, Guid userId) {
            var file = _fileRepository.GetById(model.Id);
            if (userId == file.UserId)
            {
                var result = _fileRepository.MoveIntofolder(model.Id, model.ParentId);
                if (!result)
                {
                    throw new ValidationException("File has not been moved, try again.");
                }
                else return true;
            }
            else throw new ValidationException("Operation forbidden.");
        }
    }
}