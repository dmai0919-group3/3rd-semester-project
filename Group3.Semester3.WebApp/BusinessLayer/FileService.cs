using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
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
using Microsoft.CodeAnalysis;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IFileService
    {
        /// <summary>
        /// Uploads one or more file(s)
        /// </summary>
        /// <param name="user">The user who will own the uploaded files.</param>
        /// <param name="groupId">The Guid of the group or String.Empty if file uploaded belong to user.</param>
        /// <param name="parentId">The Guid of the parent folder or String.Empty if the parent is the root directory.</param>
        /// <param name="files">A List<IFormFile> containing all the files uploaded by the user</param>
        /// <returns>A List<FileEntity> containing all the files that were uploaded</returns>
        /// <exception cref="ValidationException">If there were no files chosen</exception>
        public Task<List<FileEntry>> UploadFile(UserModel user, string groupId, string parentId, List<IFormFile> files);

        /// <summary>
        /// Gets the files owned by a given user in a given folder
        /// TODO Why is the parentId a string instead of Guid?
        /// </summary>
        /// <param name="currentUser">The user whose files we are checking</param>
        /// <param name="groupId">The Guid of group or String.Empty if browsed files do not belong to a group</param>
        /// <param name="parentId">The Guid of the parent folder or String.Empty if the parent is the root directory.</param>
        /// <returns>An IEnumerable<FileEntity> which contains the FileEntities that can be accessed by the user.</returns>
        public (UserModel user, IEnumerable<FileEntity> files) BrowseFiles(UserModel currentUser, string groupId, string parentId);

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="fileId">The Guid of the file that needs to be renamed.</param>
        /// <param name="user">The User whose file it is.</param>
        /// <param name="name">The new name of the file.</param>
        /// <returns>The FileEntity of the file including the new name.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="ValidationException">If the file doesn't exist or if there were some errors while renaming the file.</exception>
        public FileEntity RenameFile(Guid id, UserModel user, string name);

        /// <summary>
        /// Gets a file by Guid
        /// </summary>
        /// <param name="id">The Guid of the file</param>
        /// <returns>The FileEntity with the given id</returns>
        /// <exception cref="ValidationException">If there are no file matching the given id</exception>
        public FileEntity GetById(Guid id);

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="fileId">The Guid of the file that needs to be deleted.</param>
        /// <param name="user">The User whose file it is.</param>
        /// <returns>True if the file has been deleted successfully.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file</exception>
        public bool DeleteFile(Guid fileId, UserModel user);

        /// <summary>
        /// Creates a new folder
        /// </summary>
        /// <param name="user">The User who will own the new folder</param>
        /// <param name="model">The CreateFolderModel containing the details of the new folder</param>
        /// <returns>The FileEntity created for the new folder.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="Exception">If there were come errors while creating the folder.</exception>
        public FileEntity CreateFolder(UserModel user, CreateFolderModel model);

        public UserModel ShareFile(SharedFile sharedFileModel, UserModel currentUser);
        public string ShareFile(FileEntity fileEntity, UserModel currentUser);
        public FileEntity OpenSharedFileLink(string hash, UserModel currentUser);
        public bool UnShareFile(SharedFile sharedFile, UserModel currentUser);
        public bool DisableShareLink(FileEntity sharedFile, UserModel currentUser);
        public bool DisableSharing(FileEntity sharedFile, UserModel currentUser);
        public (UserModel user, IEnumerable<FileEntity> files) BrowseSharedFiles(UserModel currentUser, string parentId);
        public IEnumerable<UserModel> SharedWithList(FileEntity fileEntity, UserModel currentUser);
        public (IEnumerable<UserModel>, string) GetShareInfo(FileEntity fileEntity, UserModel currentUser);

        /// <summary>
        /// Move a file into a folder
        /// </summary>
        /// <param name="model">The FileEntity model containing the new parentId of the file</param>
        /// <param name="user">The User who is the owner of the file</param>
        /// <returns>True if the file has been moved successfully.</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="ValidationException">If there were some errors while moving the file.</exception>
        public bool MoveIntoFolder(FileEntity model, UserModel user);

        /// <summary>
        /// Generates a SAS URI for a given file
        /// </summary>
        /// <param name="fileId">The Guid of the file the user want to download</param>
        /// <param name="versionId">If we want to download different version of a file</param>
        /// <param name="user">The UserModel of the user who wants to download the file</param>
        /// <returns>A tuple with the FileEntity of the given fileId and a string with the download URL</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file</exception>
        public (FileEntity, string) DownloadFile(Guid fileId, string versionId, UserModel user);
        
        public string GenerateDownloadLink(FileEntity file, string azureName = null);

        /// <summary>
        /// Gets the contents of a PLAINTEXT file.
        /// TODO Return an exception if the selected file is NOT a plaintext file
        /// </summary>
        /// <param name="id">The ID of the file</param>
        /// <param name="user">The User who is the owner of the file</param>
        /// <returns>An UpdateFileModel matching the file with the given ID</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        public UpdateFileModel GetFileContents(string id, UserModel user);

        /// <summary>
        /// Updates the content of a PLAINTEXT file
        /// </summary>
        /// <param name="model">The UpdateFileModel containing the changes made to the file</param>
        /// <param name="user">The User who is the owner of the file</param>
        /// <returns>The FileEntity containing the changes</returns>
        /// <exception cref="ValidationException">AccessService.hasAccess() throws this exception if the user doesn't have access to download the file.</exception>
        /// <exception cref="ConcurrencyException">If the file has been changed by another user in the meantime.</exception>
        public FileEntity UpdateFileContents(UpdateFileModel model, UserModel user);

        public IEnumerable<FileVersion> GetFileVersions(Guid fileId, UserModel user);

        public FileVersion RevertFileVersion(FileVersion fileVersion, UserModel user);

        public IEnumerable<FileEntity> GetParents(Guid fileId, UserModel user);
    }

    public class FileService : IFileService
    {
        private IConfiguration _configuration;
        private IFileRepository _fileRepository;
        private IAccessService _accessService;
        private IGroupRepository _groupRepository;
        private ISharedFilesRepository _sharedFilesRepository;
        private IUserRepository _userRepository;

        public FileService(IConfiguration configuration, IFileRepository fileRepository, IAccessService accessService, 
            IGroupRepository groupRepository, ISharedFilesRepository sharedFilesRepository, IUserRepository userRepository)
        {
            _configuration = configuration;
            _fileRepository = fileRepository;
            _accessService = accessService;
            _groupRepository = groupRepository;
            _sharedFilesRepository = sharedFilesRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Gets the files owned by a given user in a given folder
        /// TODO Why is the parentId a string instead of Guid?
        /// </summary>
        /// <param name="currentUser">The user whose files we are checking</param>
        /// <param name="groupId">The Guid of the group or String.Empty if the listed files belong only to user.</param>
        /// <param name="parentId">The Guid of the parent folder or String.Empty if the parent is the root directory.</param>
        /// <returns>An IEnumerable<FileEntity> which contains the FileEntities that can be accessed by the user.</returns>
        public (UserModel user, IEnumerable<FileEntity> files) BrowseFiles(UserModel currentUser, string groupId, string parentId)
        {
            var parentGuid = ParseGuid(parentId);
            var groupGuid = ParseGuid(groupId);

            IEnumerable<FileEntity> fileList = null;

            if (groupGuid != Guid.Empty)
            {
                var group = _groupRepository.GetByGroupId(groupGuid);
                
                _accessService.HasAccessToGroup(currentUser, group);
                fileList = _fileRepository.GetByGroupIdAndParentId(groupGuid, parentGuid);
                currentUser = _groupRepository.GetUserModel(group.Id, currentUser.Id);
            }
            else
            {
                if (parentGuid != Guid.Empty)
                {
                    var file = GetById(parentGuid);
                    
                    _accessService.HasAccessToFile(currentUser, file, Permissions.Read);
                    fileList = _fileRepository.GetByParentId(parentGuid);
                }
                else
                {
                    fileList = _fileRepository.GetByUserIdAndParentId(currentUser.Id, parentGuid);
                }
                
                // All permissions to own files
                currentUser.PermissionsNumber = short.MaxValue;
            }

            return (currentUser, fileList);
        }

        /// <summary>
        /// Uploads one or more file(s)
        /// </summary>
        /// <param name="user">The user who will own the uploaded files.</param>
        /// <param name="groupId">The Guid of the group or String.Empty if file uploaded belong to user.</param>
        /// <param name="parentId">The Guid of the parent folder or String.Empty if the parent is the root directory.</param>
        /// <param name="files">A List<IFormFile> containing all the files uploaded by the user</param>
        /// <returns>A List<FileEntity> containing all the files that were uploaded</returns>
        /// <exception cref="ValidationException">If there were no files chosen</exception>
        public async Task<List<FileEntry>> UploadFile(UserModel user, string groupId, string parentId, List<IFormFile> files)
        {
            //long size = files.Sum(f => f.Length);

            var parentGuid = ParseGuid(parentId);
            var groupGuid = ParseGuid(groupId);
            
            // Check if user has access to a group
            if (groupGuid != Guid.Empty)
            {
                var group = _groupRepository.GetByGroupId(groupGuid);
                _accessService.HasAccessToGroup(user, group, Permissions.Write);
            }

            // Check if user has access to parent folder
            if (parentGuid != Guid.Empty)
            {
                var parent = GetById(parentGuid);
                _accessService.HasAccessToFile(user, parent, Permissions.Write);
            }

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
                        var existingFile = _fileRepository.GetFile(parentGuid, user.Id, groupGuid, formFile.FileName);
                        
                        await containerClient.UploadBlobAsync(blobGuid.ToString(), formFile.OpenReadStream());

                        if (existingFile == null)
                        {
                            var newFileId = Guid.NewGuid();
                            
                            fileEntries.Add(new FileEntry
                            {
                                Name = formFile.FileName,
                                Id = newFileId,
                                Parent = new DirectoryEntry { Id = parentGuid }
                            });

                            var file = new FileEntity()
                            {
                                Id = newFileId,
                                AzureName = blobGuid.ToString(),
                                Name = formFile.FileName,
                                UserId = user.Id,
                                ParentId = parentGuid,
                                GroupId = groupGuid,
                                IsFolder = false,
                                Updated = DateTime.Now,
                                Size = formFile.Length
                            };

                            _fileRepository.Insert(file);
                        }
                        else
                        {
                            var newVersion = new FileVersion()
                            {
                                Id = Guid.NewGuid(),
                                FileId = existingFile.Id,
                                AzureName = existingFile.AzureName,
                                Note = "New version uploaded",
                                Created = DateTime.Now,
                                UserId = user.Id
                            };
                            
                            existingFile.AzureName = blobGuid.ToString();
                            existingFile.Size = formFile.Length;
                            existingFile.Updated = DateTime.Now;

                            var success = _fileRepository.UpdateFileAndCreateNewVersion(existingFile, newVersion);

                            if (!success)
                            {
                                await containerClient.GetBlobClient(blobGuid.ToString()).DeleteAsync();
                            }
                        }
                        
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
        public (FileEntity, string) DownloadFile(Guid fileId, string versionId, UserModel user)
        {
            var file = _fileRepository.GetById(fileId);
            
            _accessService.HasAccessToFile(user, file, Permissions.Read);

            var azureName = file.AzureName;
            
            var versionGuid = ParseGuid(versionId);
            if (versionGuid != Guid.Empty)
            {
                var version = _fileRepository.GetFileVersion(versionGuid);
                if (version != null)
                {
                    azureName = version.AzureName;
                }
            }

            var link = GenerateDownloadLink(file, azureName);

            return (file, link);
        }

        public string GenerateDownloadLink(FileEntity file, string azureName = null)
        {
            if (string.IsNullOrEmpty(azureName))
            {
                azureName = file.AzureName;
            }
            
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
                BlobName = azureName,
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

            return $"{containerClient.GetBlockBlobClient(azureName).Uri}?{sasToken}";
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

            var file = _fileRepository.GetById(fileId);
            _accessService.HasAccessToFile(user, file, Permissions.Write);
            
            if (!file.IsFolder)
            {
                BlobContainerClient containerClient =
                    new BlobContainerClient(
                        _configuration.GetConnectionString("AzureConnectionString"),
                        _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);
                
                containerClient.DeleteBlob(_fileRepository.GetById(fileId).AzureName.ToString());
            }
            else
            {
                // TODO: Check for children and recursively delete
            }
            
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
            _accessService.HasAccessToFile(user, file, Permissions.Write);
            file.Name = name;
            var result = _fileRepository.Update(file);
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

            var parentGuid = model.ParentId;
            var groupGuid = model.GroupId;
            
            // Check if user has access to a group
            if (groupGuid != Guid.Empty)
            {
                var group = _groupRepository.GetByGroupId(groupGuid);
                _accessService.HasAccessToGroup(user, group);
            }

            // Check if user owns parent folder
            if (parentGuid != Guid.Empty)
            {
                var parent = GetById(parentGuid);
                _accessService.HasAccessToFile(user, parent, Permissions.Write);
            }

            var folder = new FileEntity()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                AzureName = null,
                UserId = user.Id,
                ParentId = parentGuid,
                GroupId = groupGuid,
                Updated = DateTime.Now,
                IsFolder = true
            };

            var created = _fileRepository.Insert(folder);

            if (!created)
            {
                throw new ValidationException("Failed to create folder");
            }

            return folder;
        }

        public (IEnumerable<UserModel>, string) GetShareInfo(FileEntity fileEntity, UserModel currentUser)
        {
            _accessService.HasAccessToFile(currentUser, fileEntity, Permissions.Administrate);

            string link = _sharedFilesRepository.GetHashByFileId(fileEntity.Id);
            var userList = _sharedFilesRepository.GetUsersByFileId(fileEntity.Id);

            return (userList, link);
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
            if (model.Id == model.ParentId)
            {
                throw new ValidationException("Can not move file into itself");
            }
            var file = GetById(model.Id);
            _accessService.HasAccessToFile(user, file, Permissions.Write);
            var result = _fileRepository.MoveIntoFolder(model.Id, model.ParentId);
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
            _accessService.HasAccessToFile(user, file, Permissions.Read);
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
            
            if (file == null)
            {
                throw new ConcurrencyException("File was deleted by another user");
            }
            
            _accessService.HasAccessToFile(user, file, Permissions.Write);

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
            
            var newVersion = new FileVersion()
            {
                Id = Guid.NewGuid(),
                FileId = file.Id,
                AzureName = file.AzureName,
                Note = "Updated file contents",
                Created = DateTime.Now,
                UserId = user.Id
            };

            file.AzureName = Guid.NewGuid().ToString();
            file.Updated = DateTime.Now;
            file.Size = contentStream.Length;

            var success = _fileRepository.UpdateFileAndCreateNewVersion(file, newVersion);
            
            if (success)
            {
                containerClient.GetBlobClient(file.AzureName).Upload(contentStream, true);
            }
            else
            {
                throw new ValidationException("Failed to update a file");
            }
            
            return file;
        }

        public IEnumerable<FileVersion> GetFileVersions(Guid fileId, UserModel user)
        {
            var file = _fileRepository.GetById(fileId);
            
            _accessService.HasAccessToFile(user, file, Permissions.Read);

            return _fileRepository.GetFileVersions(fileId);
        }

        public FileVersion RevertFileVersion(FileVersion version, UserModel user)
        {
            version = _fileRepository.GetFileVersion(version.Id);
            
            var file = _fileRepository.GetById(version.FileId);

            _accessService.HasAccessToFile(user, file, Permissions.Write);

            var containerClient =
                new BlobContainerClient(
                    _configuration.GetConnectionString("AzureConnectionString"),
                    _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

            containerClient.CreateIfNotExists();
            
            var newVersion = new FileVersion()
            {
                Id = Guid.NewGuid(),
                FileId = file.Id,
                AzureName = file.AzureName,
                Note = "Reverted file version from " + version.Created.ToString("G"),
                Created = DateTime.Now,
                UserId = user.Id,
                Username = user.Name
            };

            file.Size = containerClient.GetBlobClient(version.AzureName).GetProperties().Value.ContentLength;
            file.AzureName = version.AzureName;
            file.Updated = DateTime.Now;

            var success =_fileRepository.UpdateFileAndCreateNewVersion(file, newVersion);

            if (!success)
            {
                throw new ValidationException("Failed to revert file version");
            }
            
            return newVersion;
        }

        public IEnumerable<FileEntity> GetParents(Guid fileId, UserModel user)
        {
            if (fileId == Guid.Empty)
            {
                throw new ValidationException("File id can not be empty");
            }

            var file = _fileRepository.GetById(fileId);
            
            _accessService.HasAccessToFile(user, file, Permissions.Read);
            
            var list = new List<FileEntity>();
            
            var parents = _fileRepository.GetParents(fileId);
            list = parents.ToList();
            
            var rootFolder = new FileEntity() { Name = "Home", Id = Guid.Empty, GroupId = file.GroupId };

            list.Add(rootFolder);
            list.Reverse();
            
            return list;
        }

        public UserModel ShareFile(SharedFile sharedFile, UserModel currentUser)
        {
            
            var file = _fileRepository.GetById(sharedFile.FileId);

            var user = new User();
            
            if (sharedFile.UserId == Guid.Empty)
            {
                user = _userRepository.GetByEmail(sharedFile.Email);

                if (user == null)
                {
                    throw new ValidationException("User with this email not found");
                }

                sharedFile.UserId = user.Id;
            }
            
            
            if(file.GroupId != Guid.Empty) 
            {
                throw new ValidationException("Cannot share group files.");
            }
            _accessService.HasAccessToFile(currentUser, file, Permissions.Write);
            
            if (!file.IsShared)
            {
                file.IsShared = true;
                _fileRepository.Update(file);
            }

            var isShared = _sharedFilesRepository.IsSharedWithUser(file.Id, sharedFile.UserId);
            
            var userModel = new UserModel() {Id = user.Id, Email = user.Email, Name = user.Name};
            
            if(isShared)
            {
                return userModel;
            }
            else
            {
                _sharedFilesRepository.Insert(sharedFile);
                return userModel;
            }
        }

        public string ShareFile(FileEntity fileEntity, UserModel currentUser)
        {
            var file = _fileRepository.GetById(fileEntity.Id);
            if(file.GroupId != Guid.Empty) 
            {
                throw new ValidationException("Cannot share group files.");
            }
            _accessService.HasAccessToFile(currentUser, file, Permissions.Write);

            if (!file.IsShared)
            {
                file.IsShared = true;
                _fileRepository.Update(file);
            }
            
            var fileHash = "";
            
            using (var algorithm = SHA512.Create())
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(file.Id.ToString()));

                fileHash = Convert.ToBase64String(hashedBytes);
            }

            fileHash = RemoveSpecialCharacters(fileHash);

            var sharedFileLink = new SharedFileLink() { FileId = file.Id, Hash = fileHash};
            var fileShareExisting = _sharedFilesRepository.GetByLink(fileHash);
            
            // Can not return full url, since controllers can change
            if(fileShareExisting == null)
            {
                _sharedFilesRepository.InsertWithLink(sharedFileLink);
                return fileHash;
            }
            else
            {
                return fileHash;
            }
            
        }

        public FileEntity OpenSharedFileLink(string hash, UserModel currentUser)
        {
            var file = _sharedFilesRepository.GetByLink(hash);
            
            if (currentUser != null)
            {
                // If user owns file we do not want to share
                if (currentUser.Id != file.UserId)
                {
                    if (!_sharedFilesRepository.IsSharedWithUser(file.Id, currentUser.Id))
                    {
                        var sharedFile = new SharedFile() {FileId = file.Id, UserId = currentUser.Id};
                        _sharedFilesRepository.Insert(sharedFile);
                    }
                }
            }
            
            return file;
        }

        public bool UnShareFile(SharedFile sharedFile, UserModel currentUser)
        {

            var file = _fileRepository.GetById(sharedFile.FileId);
            _accessService.HasAccessToFile(currentUser, file, Permissions.Read);
            
            if (file.IsFolder)
            {
                var children = _fileRepository.GetFoldersByParentId(file.Id);
                foreach (var child in children)
                {
                    var childShared = new SharedFile() {FileId = child.Id, UserId = sharedFile.UserId};
                    UnShareFile(childShared, currentUser);
                }
            }
            
            return _sharedFilesRepository.DeleteBySharedFile(sharedFile);
        }

        public bool DisableShareLink(FileEntity sharedFile, UserModel currentUser)
        {
            var file = _fileRepository.GetById(sharedFile.Id);
            _accessService.HasAccessToFile(currentUser, file, Permissions.Write);

            if (file.IsFolder)
            {
                var children = _fileRepository.GetFoldersByParentId(file.Id);
                foreach (var child in children)
                {
                    if (child.IsFolder)
                    {
                        DisableShareLink(child, currentUser);
                    }
                }
            }
            
            return _sharedFilesRepository.DeleteShareLinkByFileId(file.Id);
        }

        public bool DisableSharing(FileEntity sharedFile, UserModel currentUser)
        {
            var file = _fileRepository.GetById(sharedFile.Id);
            _accessService.HasAccessToFile(currentUser, file, Permissions.Administrate);
            
            if (file.IsFolder)
            {
                var children = _fileRepository.GetFoldersByParentId(file.Id);
                foreach (var child in children)
                {
                    if (child.IsFolder)
                    {
                        DisableSharing(child, currentUser);
                    }
                }
            }
            
            _sharedFilesRepository.DeleteShareLinkByFileId(file.Id);
            _sharedFilesRepository.DeleteForAll(file.Id);

            file.IsShared = false;
            _fileRepository.Update(file);
            
            return true;
        }

        public (UserModel, IEnumerable<FileEntity>) BrowseSharedFiles(UserModel currentUser, string parentId)
        {
            var parentGuid = ParseGuid(parentId);

            IEnumerable<FileEntity> fileList;
            
            if (parentGuid != Guid.Empty)
            {
                var file = _fileRepository.GetById(parentGuid);
                _accessService.HasAccessToFile(currentUser, file, Permissions.Read);
                fileList = _fileRepository.GetByParentId(parentGuid);
            }
            else
            {
                fileList = _sharedFilesRepository.GetByUserId(currentUser.Id);
            }
            
            // TODO: In the future implement custom permission for shared files
            currentUser.PermissionsNumber = 1;

            return (currentUser, fileList);
        }

        public IEnumerable<UserModel> SharedWithList(FileEntity fileEntity, UserModel currentUser)
        {
            var file = _fileRepository.GetById(fileEntity.Id);
            
            _accessService.HasAccessToFile(currentUser, file, Permissions.Write);
            
            return _sharedFilesRepository.GetUsersByFileId(fileEntity.Id);
        }
        
        private static string RemoveSpecialCharacters(string str) {
            var sb = new StringBuilder();
            foreach (char c in str) {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}