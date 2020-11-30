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
        /// </summary>
        /// <param name="files">List of files to upload</param>
        /// <param name="parentGuid">The GUID of the parent DirectoryEntry</param>
        /// <returns>What the actual hecc does it return</returns>

        // TODO It shouldn't return bool. Make it return a list of generated FileEntities instead.
        // TODO Make it asynchronous and use callbacks to not make everything hang when there's an upload.
        public void UploadFiles(List<FileToUpload> files, System.Guid? parentGuid);

        // TODO should take a parent GUID, page, limits (with a server hard-limit), etc. Returning all the files in bulk is dangerous

        /// <summary>
        /// Retrieves all files as a list of FileEntites owned by the current user.
        /// </summary>
        /// <returns>The list of FileEntities owned by the current user</returns>
        public List<FileEntity> FileList();
    }

    /// <summary>
    /// Implementation of Api Service
    /// </summary>
    public class ApiService : IApiService
    {
        protected string BearerToken { get; set; }

        public class ApiAuthorizationException : Exception
        {
            public ApiAuthorizationException() { }
            public ApiAuthorizationException(string message) : base(message) { }
            public ApiAuthorizationException(string message, Exception inner) : base(message, inner) { }
        }

        #region Constants
        const string host = "https://localhost:5001"; // TODO move to settings

        // TODO actually move everything to settings
        private string LoginUrl = $"{host}/api/user/login";
        private string RegisterUrl = $"{host}/api/User/register";
        private string FileUploadUrl = $"{host}/api/file/upload";
        private string CurrentUserUrl = $"{host}/api/user/current";
        private string BrowseFilesUrl = $"{host}/api/file/browse";
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
        /// Get current user from http context (bearer auth)
        /// </summary>
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

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new ApiAuthorizationException("Error communicating with the server");

            return JsonConvert.DeserializeObject<UserModel>(resultContent);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
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
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public UserModel Register(RegisterModel model)
        {
            var result = this.PostRequest(RegisterUrl, model);
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
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
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="parentGuid"></param>
        public void UploadFiles(List<FileToUpload> files, System.Guid? parentGuid)
        {
            var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                content.Add(new StreamContent(File.OpenRead(file.Path)), "files", file.Name);
            }

            if (parentGuid.HasValue) content.Add(new StringContent(parentGuid?.ToString()), "parentGuid");

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
        /// 
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="parameter">Object to be serialized into a json string and sent as the content of the request</param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected HttpResponseMessage GetRequest(string requestUrl, string parameters = null)
        {
            using var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(BearerToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            if (!string.IsNullOrEmpty(parameters))
                requestUrl += "?" + parameters;

            var response = httpClient.GetAsync(requestUrl);
            response.Wait();

            return response.Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<FileEntity> FileList()
        {
            var result = GetRequest(BrowseFilesUrl, BearerToken);
            string resultContent;

            {
                var t = result.Content.ReadAsStringAsync();
                t.Wait();
                resultContent = t.Result;
            }

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Error communicating with the server");

            return JsonConvert.DeserializeObject<List<FileEntity>>(resultContent);
        }
    }
}