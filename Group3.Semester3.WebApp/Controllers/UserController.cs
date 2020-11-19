using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Group3.Semester3.WebApp.Helpers;

namespace Group3.Semester3.WebApp.Controllers
{
    // user controller which
    [Route("user")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        // defining a user service through interface to flawlessly access the db
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        // GET: User/Login
        [Route("login")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(AuthenticateModel model)
        {
            try
            {
                var result = _userService.Login(model);

                var claims = new List<Claim>
                {
                  new Claim(ClaimTypes.Name, result.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(
                  claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                addMessage("User logged in successfully");
                return RedirectToAction("Dashboard");
            }
            catch (Exception exception)
            {
                addMessage(exception.Message);
                return View();
            }
        }

        // GET: UserController/Register
        [Route("register")]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: UserController/Register
        [Route("register")]
        [HttpPost]
        [AllowAnonymous]
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

        // GET: user/dashboard
        [Route("dashboard")]
        public ActionResult Dashboard()
        {
            var user = _userService.GetFromHttpContext(HttpContext);

            ViewBag.User = user;

            return View();
        }

        [Route("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        protected void addMessage(string message)
        {
            Messenger.addMessage(message);
        }
    }
}