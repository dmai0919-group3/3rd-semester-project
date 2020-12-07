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
        public List<FileEntity> FileList(Guid parentId);

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
        /// <param name="parentId">Parent id of a folder</param>
        /// <param name="name">Name of the folder</param>
        /// <returns></returns>
        public FileEntity CreateFolder(Guid parentId, string name);
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

        /// <summary>
        /// Sends an HTTP POST request
        /// </summary>
        /// <param name="requestUrl">The URL where we want to send the request</param>
        /// <param name="parameter">Object to be serialized into a JSON string and sent as the content of the request</param>
        /// <returns>An HttpResonseMessage containing the response from the API in JSON format</returns>
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

        /// <summary>
        /// Sends an HTTP GET request
        /// </summary>
        /// <param name="requestUrl">The URL where we want to send the request</param>
        /// <param name="parameters">The part of the request in the URL after the question mark (?). Example: "key=value&key=value..." </param>
        /// <returns>An HttpResonseMessage containing the response from the API in JSON format</returns>
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

        /// <summary>
        /// Sends an HTTP DELETE request
        /// </summary>
        /// <param name="requestUrl">The URL we want to send the request to</param>
        /// <param name="id">The Guid of the object to be deleted</param>
        /// <returns>An HttpResonseMessage containing the response from the API in JSON format</returns>
        protected HttpResponseMessage DeleteRequest(string requestUrl, Guid id)
        {
            using var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(BearerToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            var content = new StringContent(JsonConvert.SerializeObject(new { id }), System.Text.Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Delete,
                requestUrl
            );

            request.Content = content;

            var response = httpClient.SendAsync(request);
            response.Wait();

            return response.Result;
        }

        /// <summary>
        /// Sends an HTTP PUT request
        /// </summary>
        /// <param name="requestUrl">The URL we want to send a request to</param>
        /// <param name="parameter">bject to be serialized into a JSON string and sent as the content of the request</param>
        /// <returns>An HttpResonseMessage containing the response from the API in JSON format</returns>
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

        /// <summary>
        /// Gets the currently logged in user through the API via the BearerToken
        /// </summary>
        /// <returns>UserModel object containing the details of the currently logged in user</returns>
        /// <exception cref="ApiAuthorizationException">If the user is not logged in or if there were an error communicating with the server.</exception>
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


        /// <summary>
        /// Authorizes a user (logs them in) and saves their token in the BearerToken variable
        /// </summary>
        /// <param name="email">The Email Address of the user</param>
        /// <param name="password">The Password of the user</param>
        public void Authorize(string email, string password)
        {

            var model = new AuthenticateModel() { Email = email, Password = password };
            var result = this.PostRequest(LoginUrl, model);

            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            var resultModel = JsonConvert.DeserializeObject<LoginResultModel>(resultContent);

            BearerToken = resultModel.Token;

            _currentLogin = resultModel;
            _currentUserModel = CurrentUser();
        }

        /// <summary>
        /// Creates a new user in the system and gets the UserModel of the newly registered user.
        /// </summary>
        /// <param name="model">A RegisterModel containing the details of the new user</param>
        /// <returns>The UserModel of the newly registered user</returns>
        /// <exception cref="ApiAuthorizationException">If there were some errors communicating with the API server</exception>
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

        /// <summary>
        /// Upload a file / multiple files through the API
        /// </summary>
        /// <param name="files">A list of FileToUpload objects</param>
        /// <param name="parentId">The Guid of the parent folder of null if the files to be uploaded are in the root directory</param>
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

        /// <summary>
        /// Gets a list of all files accessible by the user
        /// </summary>
        /// <returns>A List containing FileEntity's which can be accessed by the user</returns>
        public List<FileEntity> FileList(Guid parentId = new Guid())
        {
            var url = BrowseFilesUrl;
            if (parentId != Guid.Empty)
            {
                url += "/" + parentId;
            }

            var result = GetRequest(url);
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");

            return JsonConvert.DeserializeObject<List<FileEntity>>(resultContent);
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="file">A FileEntity object to be deleted</param>
        public void DeleteFile(FileEntity file)
        {
            var result = DeleteRequest(DeleteFileUrl, file.Id);

            if (!result.IsSuccessStatusCode)
                throw new ApiAuthorizationException("Error communicating with the server");
        }

        /// <summary>
        /// Renames a file to a given new name
        /// </summary>
        /// <param name="file">A FileEntity object to be renamed</param>
        /// <param name="name">The new name of the file</param>
        /// <returns>A FileEntity object with the new name</returns>
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

        /// <summary>
        /// Creates a folder with a given name and parent
        /// </summary>
        /// <param name="parentId">The Guid of the parent folder or null if the new folder is in the root directory</param>
        /// <param name="name">The name of the new folder</param>
        /// <returns>A FileEntity corresponding to the newly created folder</returns>
        /// <exception cref="Exception">If there were some errors communicating with the server</exception>
        public FileEntity CreateFolder(Guid parentId, string name)
        {
            var content = new FileEntity() { Name = name, ParentId = parentId };

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
    }
}