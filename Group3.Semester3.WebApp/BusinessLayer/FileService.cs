using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
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
        public (FileEntity, string) DownloadFile(Guid fileId, UserModel user);
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

        /// <summary>
        /// Gets the files owned by a given user in a given folder
        /// TODO Why is the parentId a string instead of Guid?
        /// </summary>
        /// <param name="currentUser">The user whose files we are checking</param>
        /// <param name="parentId">The Guid of the parent folder or String.Empty if the parent is the root directory.</param>
        /// <returns>An IEnumerable<FileEntity> which contains the FileEntities that can be accessed by the user.</returns>
        public IEnumerable<FileEntity> BrowseFiles(UserModel currentUser, string parentId)
        {
            var parentGuid = ParseGuid(parentId);

            var fileList = _fileRepository.GetByUserIdAndParentId(currentUser.Id, parentGuid);

            return fileList;
        }

        /// <summary>
        /// Uploads one or more file(s)
        /// </summary>
        /// <param name="user">The user who will own the uploaded files.</param>
        /// <param name="parentGUID">The Guid of the parent folder or String.Empty if the parent is the root directory.</param>
        /// <param name="files">A List<IFormFile> containing all the files uploaded by the user</param>
        /// <returns>A List<FileEntity> containing all the files that were uploaded</returns>
        /// <exception cref="ValidationException">If there were no files chosen</exception>
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

            return fileEntries;
        }

        /// <summary>
        /// Generates a SAS URI for a given file
        /// </summary>
        /// <param name="fileId">The Guid of the file the user want to download</param>
        /// <param name="user">The UserModel of the user who wants to download the file</param>
        /// <returns>A tuple with the FileEntity of the given fileId and a string with the download URL</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file</exception>
        public (FileEntity, string) DownloadFile(Guid fileId, UserModel user)
        {
            var file = _fileRepository.GetById(fileId);
            _accessService.hasAccess(user, file);
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
                Resource = "b",
                ContentDisposition = new ContentDisposition()
                {
                    FileName = file.Name
                }.ToString()
            };

            blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            StorageSharedKeyCredential storageSharedKeyCredential = new StorageSharedKeyCredential(
                _configuration.GetSection("AppSettings").Get<AppSettings>().AzureStorageAccount,
                _configuration.GetSection("AppSettings").Get<AppSettings>().AzureAccountKey);

            string sasToken = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();

            return (file, $"{containerClient.GetBlockBlobClient(file.AzureName).Uri}?{sasToken}");
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="fileId">The Guid of the file that needs to be deleted.</param>
        /// <param name="user">The User whose file it is.</param>
        /// <returns>True if the file has been deleted successfully.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file</exception>
        public bool DeleteFile(Guid fileId, UserModel user)
        {
            BlobContainerClient containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            var file = _fileRepository.GetById(fileId);
            _accessService.hasAccess(user, file);
            containerClient.DeleteBlob(_fileRepository.GetById(fileId).AzureName.ToString());
            var result = _fileRepository.Delete(fileId);

            if (!result)
            {
                throw new ValidationException("File non-existent or not deleted.");
            }
            else return result;
        }

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="fileId">The Guid of the file that needs to be renamed.</param>
        /// <param name="user">The User whose file it is.</param>
        /// <param name="name">The new name of the file.</param>
        /// <returns>The FileEntity of the file including the new name.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="ValidationException">If the file doesn't exist or if there were some errors while renaming the file.</exception>
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

        /// <summary>
        /// Parses a Guid from a string.
        /// </summary>
        /// <param name="guid">A string containing a valid formatted Guid</param>
        /// <returns>A Guid object matching the given guid. If the given guid is not a valid Guid (eg.: it's a String.Empty or anything not in the valid Guid format) it returns Guid.Empty</returns>
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

        /// <summary>
        /// Gets a file by Guid
        /// </summary>
        /// <param name="id">The Guid of the file</param>
        /// <returns>The FileEntity with the given id</returns>
        /// <exception cref="ValidationException">If there are no file matching the given id</exception>
        public FileEntity GetById(Guid id)
        {
            var file = _fileRepository.GetById(id);
            if (file == null)
            {
                throw new ValidationException("No file found.");
            }
            else return file;
        }

        /// <summary>
        /// Creates a new folder
        /// </summary>
        /// <param name="user">The User who will own the new folder</param>
        /// <param name="model">The CreateFolderModel containing the details of the new folder</param>
        /// <returns>The FileEntity created for the new folder.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="Exception">If there were come errors while creating the folder.</exception>
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

        /// <summary>
        /// Move a file into a folder
        /// </summary>
        /// <param name="model">The FileEntity model containing the new parentId of the file</param>
        /// <param name="user">The User who is the owner of the file</param>
        /// <returns>True if the file has been moved successfully.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="ValidationException">If there were some errors while moving the file.</exception>
        public bool MoveIntoFolder(FileEntity model, UserModel user)
        {
            var file = GetById(model.Id);
            _accessService.hasAccess(user, file);
            var result = _fileRepository.MoveIntofolder(model.Id, model.ParentId);
            if (!result)
            {
                throw new ValidationException("File has not been moved, try again.");
            }
            else return true;
        }

        /// <summary>
        /// Gets the contents of a PLAINTEXT file.
        /// TODO Return an exception if the selected file is NOT a plaintext file
        /// </summary>
        /// <param name="id">The ID of the file</param>
        /// <param name="user">The User who is the owner of the file</param>
        /// <returns>An UpdateFileModel matching the file with the given ID</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        public UpdateFileModel GetFileContents(string id, UserModel user)
        {
            var fileId = ParseGuid(id);

            var file = _fileRepository.GetById(fileId);
            _accessService.hasAccess(user, file);
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

        /// <summary>
        /// Updates the content of a PLAINTEXT file
        /// </summary>
        /// <param name="model">The UpdateFileModel containing the changes made to the file</param>
        /// <param name="user">The User who is the owner of the file</param>
        /// <returns>The FileEntity containing the changes</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="ConcurrencyException">If the file has been changed by another user in the meantime.</exception>
        public FileEntity UpdateFileContents(UpdateFileModel model, UserModel user)
        {
            var file = _fileRepository.GetById(model.Id);
            _accessService.hasAccess(user, file);

            if (!model.Overwrite)
            {
                var result = DateTime.Compare(model.Timestamp, file.Updated);

                if (result <= 0)
                {
                    throw new ConcurrencyException("File was changed by another user. Please try again");
                }
            }

            byte[] byteArray = Encoding.ASCII.GetBytes(model.Contents);
            var contentStream = new MemoryStream(byteArray);

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