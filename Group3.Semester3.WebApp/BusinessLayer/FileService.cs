﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public FileEntity RenameFile(Guid id, UserModel user, string name);
        public FileEntity GetById(Guid id);
        public bool DeleteFile(Guid fileId, UserModel user);
        public FileEntity CreateFolder(UserModel user, CreateFolderModel model);
        public bool MoveIntoFolder(FileEntity model, UserModel user);
        public (FileEntity, string) DownloadFile(Guid fileId, Guid userId);
        public UpdateFileModel GetFileContents(string id, UserModel user);
        public FileEntity UpdateFileContents(UpdateFileModel model, UserModel user);
    }
    
    public class FileService : IFileService
    {
        private IConfiguration _configuration;
        private IFileRepository _fileRepository;
        private IAccessService _accessService;

        public FileService(IFileRepository fileRepository, IAccessService accessService)
        {
            _fileRepository = fileRepository;
            _accessService = accessService;
        }

        public FileService(IConfiguration configuration, IFileRepository fileRepository, IAccessService accessService)
        {
            _configuration = configuration;
            _fileRepository = fileRepository;
            _accessService = accessService;
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
                            IsFolder = false,
                            Updated = DateTime.Now
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
                    BlobName = file.AzureName,
                    Resource = "b"
                };

                blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                StorageSharedKeyCredential storageSharedKeyCredential = new StorageSharedKeyCredential(
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureStorageAccount,
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureAccountKey);

                string sasToken = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();

                return (file, $"{containerClient.GetBlockBlobClient(file.AzureName).Uri}?{sasToken}");
            }
            else throw new ValidationException("Operation forbidden.");
        }

        public bool DeleteFile(Guid fileId, UserModel user)
        {
            BlobContainerClient containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            var file = _fileRepository.GetById(fileId);
            if (user.Id == file.UserId)
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

        public FileEntity RenameFile(Guid fileId, UserModel user, string name)
        {
            var file = GetById(fileId);
            _accessService.hasAccess(user, file);
            var result = _fileRepository.Rename(fileId, name);
            if (!result)
            {
                throw new ValidationException("File non-existent or not renamed.");
            }
            else return GetById(fileId);
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
                _accessService.hasAccess(user, parent);
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

        public bool MoveIntoFolder(FileEntity model, UserModel user) {
            var file = GetById(model.Id);
            _accessService.hasAccess(user, file);
            var result = _fileRepository.MoveIntofolder(model.Id, model.ParentId);
            if (!result)
            {
                throw new ValidationException("File has not been moved, try again.");
            }
            else return true;
        }

        public UpdateFileModel GetFileContents(string id, UserModel user)
        {
            var fileId = ParseGuid(id);
            
            var file = _fileRepository.GetById(fileId);
            
            if (file.UserId != user.Id)
            {
                throw new ValidationException("Unauthorized");
            }
            
            var containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            containerClient.CreateIfNotExists();

            var response = containerClient.GetBlobClient(file.AzureName).Download();
            var stream = response.Value.Content;

            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            var model = new UpdateFileModel()
            {
                Id = file.Id,
                Contents = text,
                Timestamp = DateTime.Now
            };

            return model;
        }

        public FileEntity UpdateFileContents(UpdateFileModel model, UserModel user)
        {
            var file = _fileRepository.GetById(model.Id);

            if (!model.Overwrite)
            {
                var result = DateTime.Compare(model.Timestamp, file.Updated);

                if (result <= 0)
                {
                    throw new ConcurrencyException("File was changed by another user. Please try again");
                }
            }

            if (file.UserId != user.Id)
            {
                throw new ValidationException("Unauthorized");
            }
            
            byte[] byteArray = Encoding.ASCII.GetBytes( model.Contents );
            var contentStream = new MemoryStream( byteArray );

            var containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            containerClient.CreateIfNotExists();

            // Trigger update change
            _fileRepository.Rename(file.Id, file.Name);

            containerClient.GetBlobClient(file.AzureName).Upload(contentStream, true);

            return file;
        }
    }
}