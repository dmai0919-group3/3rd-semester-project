using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.DesktopClient.Model;
using System.IO;
using Group3.Semester3.DesktopClient.Helpers;
using System.Net.Http.Headers;
using Group3.Semester3.WebApp.Entities;

namespace Group3.Semester3.DesktopClient.Services
{
    /* 
     * interface for an API service 
     */ 
    public interface IApiService
    {
        public LoginResultModel Login(string email, string password);

        public UserModel Register(RegisterModel model);

        public bool UploadFiles(List<FileToUpload> files, string parentGuid);

        public List<FileEntity> FileList(UserModel currentUser);
    }

    /* 
     * implementation of API service
     */

    public class ApiService : IApiService
    {
        // 2 constants, pointing at endpoints for login and registration 

        private const string LoginUrl = "https://localhost:44306/api/User/login";
        private const string RegisterUrl = "https://localhost:44306/api/User/register";
        private const string FileUploadUrl = "https://localhost:44306/api/file/upload";
        private const string CurrentUserUrl = "https://localhost:44306/api/User/current";
        private const string BrowseFilesUrl = "https://localhost:44306/api/User/browse";


        public UserModel CurrentUser()
        {
            var result = this.GetRequest(CurrentUserUrl, BearerToken.Token);
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
                    throw new Exception(JObject.Parse(resultContent).SelectToken("message").Value<string>());
                }

                throw new Exception("Error communicating with the server");
            }

            var resultModel = JsonConvert.DeserializeObject<UserModel>(resultContent);

            return resultModel;
        }
        
        
        public LoginResultModel Login(string email, string password)
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

            BearerToken.Token = resultModel.Token;

            return resultModel;
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

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(JObject.Parse(resultContent).SelectToken("message").Value<string>());
                }

                throw new Exception("Error communicating with the server");
            }

            return JsonConvert.DeserializeObject<UserModel>(resultContent);
        }

        public bool UploadFiles(List<FileToUpload> files, string parentGuid)
        {
            try
            {
                var content = new MultipartFormDataContent();
                
                foreach (var file in files)
                {
                    content.Add(new StreamContent(File.OpenRead(file.Path)), "files", file.Name);
                }
                
                content.Add(new StringContent(parentGuid),"parentGuid");

                var client = new HttpClient();
                
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post, 
                    FileUploadUrl
                    );

                request.Content = content;
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken.Token);

                var response = client.SendAsync(request);
                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    return true;
                } 
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                return false;
            }
        }
        
        // method for posting a http request, taking url and model as a parameters
        // model object is serialized via json, the request is then posted async-ly
        // if the http response is 200 OK, the response is saved as string and returned
        // if the response is not 200 OK, an exception is thrown with a status code
        protected HttpResponseMessage PostRequest(string url, object parameter)
        {
            var httpClient = new HttpClient();
            
            var content = new StringContent(JsonConvert.SerializeObject(parameter), System.Text.Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync(url, content);
            response.Wait();

            return response.Result;
        }

        protected HttpResponseMessage GetRequest(string url, string token = "", string parameter = "key=value,key=value")
        {
            var httpClient = new HttpClient();

            string requestUrl = url;

            if (token  != "") 
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (parameter != "") 
                requestUrl += "?" + parameter;

            var response = httpClient.GetAsync(url);
            response.Wait();

            if (!response.Result.IsSuccessStatusCode)
            {
                throw new Exception("Request " + url + " failed with status code " + response.Result.StatusCode);
            }

            return response.Result;
        }

        public List<FileEntity> FileList(UserModel currentUser)
        {
            
        }
    }
}