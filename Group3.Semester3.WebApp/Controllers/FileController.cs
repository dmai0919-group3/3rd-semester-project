using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Group3.Semester3.WebApp.BusinessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Entities;
using System;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("file")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class FileController : Controller
    {

        [Route("browse")]
        [HttpGet]
        public ActionResult Browse()
        {
            return View();
        }

    }
}
