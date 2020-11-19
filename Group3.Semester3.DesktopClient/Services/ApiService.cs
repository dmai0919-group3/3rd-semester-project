using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Group3.Semester3.WebApp.Models.Users;

namespace Group3.Semester3.DesktopClient.Services
{
    /* 
     * interface for an API service 
     */ 
    public interface IApiService
    {
        public LoginResultModel Login(string email, string password);

        public UserModel Register(RegisterModel model);
    }

    /* 
     * implementation of API service
     */

    public class ApiService : IApiService
    {
        // 2 constants, pointing at endpoints for login and registration 

        private const string LoginUrl = "https://localhost:44306/api/User/login";
        private const string RegisterUrl = "https://localhost:44306/api/User/register";
        
        
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
        // method for posting a http request, taking url and model as a parameters
        // model object is serialized via json, the request is then posted async-ly
        // if the http response is 200 OK, the response is saved as string and returned
        // if the response is not 200 OK, an exception is thrown with a status code
        protected string PostRequest(string url, object parameter)
        {
            var httpClient = new HttpClient();
            
            var content = new StringContent(JsonConvert.SerializeObject(parameter));

            var response = httpClient.PostAsync(url, content);
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