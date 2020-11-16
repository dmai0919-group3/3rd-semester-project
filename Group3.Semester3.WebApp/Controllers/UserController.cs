﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Services;
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
                var result = _userService.Register(model);
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