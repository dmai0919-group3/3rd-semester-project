using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("user")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        /// <summary>
        /// GET: user/login
        /// </summary>
        /// <returns>A view of the login page</returns>
        [Route("login")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// POST: user/login
        /// </summary>
        /// <param name="model">The AuthenticateModel of the user who wants to log in</param>
        /// <returns>Redirects the user to the Dashboard if logged in or shows an error message and stays on the same page</returns>
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

                AddMessage("User logged in successfully");
                return RedirectToAction("Dashboard");
            }
            catch (ValidationException e)
            {
                AddMessage(e.Message);
                return View();
            }
            catch
            {
                AddMessage(Messages.SystemError);
                return View();
            }
        }

        /// <summary>
        /// GET: user/register
        /// </summary>
        /// <returns>A view of the register page</returns>
        [Route("register")]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: user/register
        /// </summary>
        /// <param name="model">The RegisterModel of the new user</param>
        /// <returns>Shows a message that the user registered successfully or an error message and stays on the page.</returns>
        [Route("register")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            try
            {
                _userService.Register(model);
                AddMessage("User registered successfully");
            }
            catch (ValidationException e)
            {
                AddMessage(e.Message);
            }

            return View();
        }

        /// <summary>
        /// GET: user/dashboard
        /// </summary>
        /// <returns>Checks if the user is logged in, if yes shows their details or redirects the user to the login page</returns>
        [Route("dashboard")]
        public ActionResult Dashboard()
        {
            try {
                var user = _userService.GetFromHttpContext(HttpContext);
                ViewBag.User = user;
            }
            catch(ValidationException e)
            {
                AddMessage(e.Message);
            }
            return View();
        }

        /// <summary>
        /// GET: user/logout
        /// </summary>
        /// <returns>Redirects the user to the login page</returns>
        [Route("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        /// <summary>
        /// GET: user/dashboard
        /// </summary>
        /// <returns>Checks if the credentials were altered, if yes alters them and redirects the user to the login page</returns>
        [Route("update")]
        public ActionResult Update(UserUpdateModel newUser)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                _userService.Update(newUser, user);
                AddMessage("User updated successfully");
            }
            catch (ValidationException e)
            {
                AddMessage(e.Message);
            }
            
            return RedirectToAction("Dashboard");
        }

        /// <summary>
        /// Adds a new message to the Messenger
        /// </summary>
        /// <param name="message">The message to be added to the Messenger</param>
        private void AddMessage(string message)
        {
            Messenger.addMessage(message);
        }
    }
}