using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.BusinessLayer;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {

        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
                var result = _userService.Login(model);
                addMessage("User logged in successfully");
                return View();
            }
            catch (Exception exception)
            {
                addMessage(exception.Message);
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
                var result = _userService.Register(model);
                addMessage("User registered successfully");
                return View();
            }
            catch (Exception exception)
            {
                addMessage(exception.Message);
                return View();
            }
        }

        protected void addMessage(string message)
        {
            if (ViewBag.Messages == null)
            {
                ViewBag.Messages = new List<string>();
            }

            ViewBag.Messages.Add(message);
        }
    }
}