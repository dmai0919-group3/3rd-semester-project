using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
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
        const string host = "http://localhost:5000"; // TODO move to settings

        // TODO actually move everything to settings
        private string LoginUrl = $"{host}/api/User/login";
        private string RegisterUrl = $"{host}/api/User/register";
        private string FileUploadUrl = $"{host}/api/file/upload";
        private string CurrentUserUrl = $"{host}/api/User/current";
        private string BrowseFilesUrl = $"{host}/api/User/browse";


        public UserModel CurrentUser()
        {
            var result = this.GetRequest(CurrentUserUrl, BearerToken.Token);

            var resultModel = JsonConvert.DeserializeObject<UserModel>(result);

            return resultModel;
        }
        
        
        public LoginResultModel Login(string email, string password)
        {
            var model = new AuthenticateModel() { Email = email, Password = password };

            var result = this.PostRequest(LoginUrl, model);

            var resultModel = JsonConvert.DeserializeObject<LoginResultModel>(result);

            BearerToken.Token = resultModel.Token;

            return resultModel;
        }

        public UserModel Register(RegisterModel model)
        {
            try
            {
                var result = this.PostRequest(RegisterUrl, model);

                var resultModel = JsonConvert.DeserializeObject<UserModel>(result);

                return resultModel;
            } 
            catch (Exception exception) //TODO Are we sure we just catch a general exception and don't do anything with it?
            {
                return null;
            }
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
        protected string PostRequest(string url, object parameter)
        {
            var httpClient = new HttpClient();
            
            var content = new StringContent(JsonConvert.SerializeObject(parameter), System.Text.Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync(url, content);
            response.Wait();

            var result = response.Result.Content.ReadAsStringAsync();
            result.Wait();

            if (!response.Result.IsSuccessStatusCode)
            {
                throw new Exception("Request "+url+" failed with status code "+ response.Result.StatusCode);
            }


            return result.Result;
        }

        protected string GetRequest(string url, string token = "", string parameter = "key=value,key=value")
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

            var result = response.Result.Content.ReadAsStringAsync();
            result.Wait();

            return result.Result;
        }

        public List<FileEntity> FileList(UserModel currentUser)
        {
            throw new NotImplementedException();
        }
    }
}