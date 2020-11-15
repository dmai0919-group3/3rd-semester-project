using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using web_app.Models.Users;

namespace web_app.Services
{
    public interface IApiService
    {
        public LoginResultModel Login(string email, string password);
    }
    
    public class ApiService : IApiService
    {
        private const string LoginUrl = "user/login";
        
        
        public LoginResultModel Login(string email, string password)
        {
            var model = new AuthenticateModel() { Email = email, Password = email };

            var result = this.PostRequest(LoginUrl, model);

            var resultModel = JsonConvert.DeserializeObject<LoginResultModel>(result);

            return resultModel;
        }


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