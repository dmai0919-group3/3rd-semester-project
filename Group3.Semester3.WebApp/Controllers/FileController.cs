using Group3.Semester3.WebApp.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("file")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class FileController : Controller
    {
        private IFileService _fileService;
        private IUserService _userService;

        public FileController(IFileService fileService, IUserService userService)
        {
            _fileService = fileService;
            _userService = userService;
        }


        /// <summary>
        /// GET: file/browse
        /// </summary>
        /// <returns>A View showing the user's files</returns>
        [Route("browse")]
        [HttpGet]
        public ActionResult Browse()
        {
            return View();
        }

        /// <summary>
        /// GET: file/shared/{hash}
        /// </summary>
        /// <returns>A view for anonymous users or redirect for logged in users</returns>
        [Route("shared/{hash}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult SharedFileLink(string hash)
        {
            var user = _userService.GetFromHttpContext(HttpContext);

            var file = _fileService.OpenSharedFileLink(hash, user);

            if (user != null)
            {
                return RedirectToAction("Browse");
            }

            ViewBag.File = file;
            
            return null;
        }

    }
}
