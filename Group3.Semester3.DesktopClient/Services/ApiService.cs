using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.DesktopClient.Model;
using System.IO;
using System.Net.Http.Headers;
using Group3.Semester3.WebApp.Entities;
using System.Security;
using Group3.Semester3.WebApp.Models.FileSystem;

namespace Group3.Semester3.DesktopClient.Services
{
    /// <summary>
    /// Api Service
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// The UserModel corresponding to the active session containing information of the current user 
        /// </summary>
        public UserModel User { get; }

        /// <summary>
        /// The LoginResultModel corresponding to the active session containing session information
        /// </summary>
        public LoginResultModel Login { get; }

        /// <summary>
        /// Requests a valid bearer token from the server and sets the Login and User fields
        /// if matching credentials are found
        /// </summary>
        /// <param name="email">Email of the registered user</param>
        /// <param name="password">Password of the registered user</param>
        public void Authorize(string email, string password);

        /// <summary>
        /// Registers a user and generates a corresponding UserModel if the fields set in the model are valid
        /// and the value of the email field is unique
        /// </summary>
        /// <param name="registerModel">The RegisterModel containing the request parameters</param>
        /// <returns>The generated UserModel</returns>
        public UserModel Register(RegisterModel registerModel);

        /// <summary>
        /// Pushes a set of files to the database
        /// TODO It shouldn't return bool. Make it return a list of generated FileEntities instead.
        /// TODO Make it asynchronous and use callbacks to not make everything hang when there's an upload.
        /// </summary>
        /// <param name="files">List of files to upload</param>
        /// <param name="parentGuid">The GUID of the parent DirectoryEntry</param>
        /// <returns>What the actual hecc does it return</returns> //TODO
        public void UploadFiles(List<FileToUpload> files, System.Guid parentGuid);

        /// <summary>
        /// Retrieves all files as a list of FileEntites owned by the current user
        /// TODO should take a parent GUID, page, limits (with a server hard-limit), etc. Returning all the files in bulk is dangerous
        /// </summary>
        /// <param name="parentId">Guid of parent folder. Use Guid.Empty for root folder</param>
        /// <returns>The list of FileEntities owned by the current user</returns>
        public List<FileEntity> FileList(Guid parentId, Guid groupId);

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="file">The FileEntity object that needs to be deleted from the DB and Azure</param>
        public void DeleteFile(FileEntity file);

        /// <summary>
        /// Renames a file and returns a new FileEntity object with the correct name
        /// </summary>
        /// <param name="file">The FileEntity containing the file with the old name</param>
        /// <param name="name">The String containing the new name of the file</param>
        /// <returns>The new FileEntity object with the new name</returns>
        public FileEntity RenameFile(FileEntity file, String name);

        /// <summary>
        /// Creates a new folder
        /// </summary>
        /// <param name="parentId">Id of the parent folder</param>
        /// <param name="name">Name of the folder</param>
        /// <param name="groupId">Id of the parent group</param>
        /// <returns></returns>
        public FileEntity CreateFolder(string name, Guid parentId, Guid groupId);

        /// <summary>
        /// Gets the list of groups associated with the current user
        /// </summary>
        /// <returns>The list of groups associated with the current user</returns>
        public List<Group> GetGroups();

        /// <summary>
        /// Retrieves a list of groups the current user is a member of
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>The list of groups the current user is a member of</returns>
        public List<UserModel> GetGroupUsers(Guid groupId);

        /// <summary>
        /// Removes a group
        /// </summary>
        /// <param name="groupId"></param>
        public void RemoveGroup(Guid groupId);
    }

    /// <summary>
    /// Implementation of Api Service
    /// </summary>
    public class ApiService : IApiService
    {
        protected string BearerToken { get; set; }

        /// <summary>
        /// This exception is thrown when the user doesn't have access to an API request
        /// </summary>
        public class ApiAuthorizationException : Exception
        {
            public ApiAuthorizationException() { }
            public ApiAuthorizationException(string message) : base(message) { }
            public ApiAuthorizationException(string message, Exception inner) : base(message, inner) { }
        }

        #region Constants // TODO move everything to settings
        const string host = "https://localhost:5001";

        private string LoginUrl = $"{host}/api/user/login";
        private string RegisterUrl = $"{host}/api/User/register";
        private string FileUploadUrl = $"{host}/api/file/upload";
        private string CurrentUserUrl = $"{host}/api/user/current";
        private string BrowseFilesUrl = $"{host}/api/file/browse";
        private string DeleteFileUrl = $"{host}/api/file/delete";
        private string RenameFileUrl = $"{host}/api/file/rename";
        private string CreateFolderUrl = $"{host}/api/file/create-folder";
        private string GetLinkUrl = $"{host}/api/file/download";
        private string GetGroupUrl = $"{host}/api/group";
        private string GetGroupUsersUrl = $"{host}/api/group/get-users";
        private string RemoveGroupUrl = $"{host}/api/group/delete";
        private string AddGroupUrl = $"{host}/api/group/create-group";
        private string AddGroupUserUrl = $"{host}/api/group/add-user/add-user";
        private string RemoveGroupUserUrl = $"{host}/api/group/remove-user";
        #endregion

        protected UserModel _currentUserModel;
        protected LoginResultModel _currentLogin;

        public UserModel User
        {
            get => _currentUserModel;
        }

        LoginResultModel IApiService.Login
        {
            get => _currentLogin;
        }

        protected HttpResponseMessage PostRequest(string requestUrl, object parameter = null)
        {
            using var httpClient = new HttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(parameter), System.Text.Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(BearerToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            var response = httpClient.PostAsync(requestUrl, content);
            response.Wait();

            return response.Result;
        }

        protected HttpResponseMessage GetRequest(string requestUrl, string parameters = null)
        {
            using var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(BearerToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            if (!string.IsNullOrEmpty(parameters))
                requestUrl += parameters;

            var response = httpClient.GetAsync(requestUrl);
            response.Wait();

            return response.Result;
        }

        protected HttpResponseMessage PutRequest(string requestUrl, object parameter)
        {
            using var httpClient = new HttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(parameter), System.Text.Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(BearerToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            var response = httpClient.PutAsync(requestUrl, content);
            response.Wait();

            return response.Result;
        }

        private UserModel CurrentUser()
        {
            var result = this.GetRequest(CurrentUserUrl);
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new ApiAuthorizationException(JObject.Parse(resultContent).SelectToken("message").Value<string>());

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            return JsonConvert.DeserializeObject<UserModel>(resultContent);
        }

        public void Authorize(string email, string password)
        {

            var model = new AuthenticateModel() { Email = email, Password = password };
            var result = PostRequest(LoginUrl, model);

            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
            {
                try
                {
                    string message = JObject.Parse(resultContent).SelectToken("message").ToString();
                    throw new ApiAuthorizationException(message);
                }
                catch
                {
                    throw;
                }
            }

            var resultModel = JsonConvert.DeserializeObject<LoginResultModel>(resultContent);

            BearerToken = resultModel.Token;

            _currentLogin = resultModel;
            _currentUserModel = CurrentUser();
        }

        public UserModel Register(RegisterModel model)
        {
            var result = this.PostRequest(RegisterUrl, model);
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ApiAuthorizationException(JObject.Parse(resultContent).SelectToken("message").Value<string>());
                }

                throw new ApiAuthorizationException("Error communicating with the server");
            }

            return JsonConvert.DeserializeObject<UserModel>(resultContent);
        }

        public void UploadFiles(List<FileToUpload> files, System.Guid parentId)
        {
            var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                content.Add(new StreamContent(File.OpenRead(file.Path)), "files", file.Name);
            }

            content.Add(new StringContent(parentId.ToString()), "parentId");

            using var client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                FileUploadUrl
                );

            request.Content = content;

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            var response = client.SendAsync(request);
            response.Wait();

        }

        public class BrowseResult { public UserModel user; public List<FileEntity> files; };

        public List<FileEntity> FileList(Guid parentId = new Guid(), Guid groupId = new Guid())
        {
            var result = GetRequest(BrowseFilesUrl, $"?groupId={groupId}&parentId={parentId}");
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            var browseResult = JsonConvert.DeserializeObject<BrowseResult>(resultContent);
            
            // TODO: Return BrowseResult with the user later, when permissions get implemented
            
            return browseResult.files;
        }

        public void DeleteFile(FileEntity file)
        {
            var result = PostRequest(DeleteFileUrl, file);

            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");
        }

        public FileEntity RenameFile(FileEntity file, string name)
        {
            FileEntity renamedFile = new FileEntity();
            renamedFile.Id = file.Id;
            renamedFile.Name = name;

            var result = PutRequest(RenameFileUrl, renamedFile);
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            return JsonConvert.DeserializeObject<FileEntity>(resultContent);
        }

        public class DownloadArgs { public FileEntity file; public string downloadLink; };

        public DownloadArgs GetDownloadLink(Guid fileId)
        {
            var response = GetRequest($"{GetLinkUrl}/{fileId.ToString()}");

            string responseContent;

            {
                var t = response.Content.ReadAsStringAsync();
                t.Wait();
                responseContent = t.Result;
            }

            if (!response.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            DownloadArgs r = JsonConvert.DeserializeObject<DownloadArgs>(responseContent);
            return r;
            
        }

        public FileEntity CreateFolder(string name, Guid parentId = new Guid(), Guid groupId = new Guid())
        {
            var content = new CreateFolderModel()
            {
                Name = name,
                ParentId = parentId,
                GroupId = groupId
            };

            var result = PostRequest(CreateFolderUrl, content);

            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            return JsonConvert.DeserializeObject<FileEntity>(resultContent);
        }

        public List<Group> GetGroups()
        {
            var response = GetRequest(GetGroupUrl);

            string responseContent;

            {
                var t = response.Content.ReadAsStringAsync();
                t.Wait();
                responseContent = t.Result;
            }

            if (!response.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            List<Group> r = JsonConvert.DeserializeObject<List<Group>>(responseContent);
            return r;
        }

        public List<UserModel> GetGroupUsers(Guid groupId)
        {
            var response = GetRequest($"{GetGroupUsersUrl}?groupId={groupId}");

            string responseContent;

            {
                var t = response.Content.ReadAsStringAsync();
                t.Wait();
                responseContent = t.Result;
            }

            if (!response.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            List<UserModel> r = JsonConvert.DeserializeObject<List<UserModel>>(responseContent);
            return r;
        }

        public void RemoveGroup(Guid groupId)
        {
            var response = PostRequest(RemoveGroupUrl, groupId);

            if (!response.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");
        }
    }
}