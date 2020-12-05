using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

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
