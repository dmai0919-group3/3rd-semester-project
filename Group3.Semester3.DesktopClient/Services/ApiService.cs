using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.DesktopClient.Model;
using System.IO;
using Group3.Semester3.DesktopClient.Helpers;
using System.Net.Http.Headers;

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
        
        public LoginResultModel Login(string email, string password)
        {
            var model = new AuthenticateModel() { Email = email, Password = email };

            var result = this.PostRequest(LoginUrl, model);

            var resultModel = JsonConvert.DeserializeObject<LoginResultModel>(result);

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
            catch (Exception exception)
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
            
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, 
                url
            );
            
            var content = new StringContent(JsonConvert.SerializeObject(parameter));

            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken.Token);
            
            var response = httpClient.SendAsync(request);
            response.Wait();

            var result = response.Result;

            if (!response.Result.IsSuccessStatusCode)
            {
                throw new Exception("Request "+url+" failed with status code "+ response.Result.StatusCode);
            }

            return response.Result.ToString();
        }
    }
}