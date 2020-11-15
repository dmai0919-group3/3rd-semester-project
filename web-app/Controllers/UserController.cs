using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using web_app.Models.Users;
using web_app.Services;

namespace web_app.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {

        private IApiService _apiService;

        public UserController(IApiService apiService)
        {
            _apiService = apiService;
        }
        
        // GET: User/Login
        [Route("login")]
        public ActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [Route("login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AuthenticateModel model)
        {
            try
            {
                var result = _apiService.Login(model.Email, model.Password);
                ViewBag.Result = result;
                return RedirectToAction(nameof(Login));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Register
        [Route("register")]
        public ActionResult Register()
        {
            return View();
        }

        // POST: UserController/Register
        [Route("register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            try
            {
                var result = _apiService.Register(model);
                ViewBag.Result = result;
                return RedirectToAction(nameof(Register));
            }
            catch
            {
                return View();
            }
        }
    }
}
