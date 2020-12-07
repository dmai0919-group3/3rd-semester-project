using System;
using Group3.Semester3.WebApp.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("group")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class GroupController : Controller
    {
        private IGroupService _groupService;
        private IUserService _userService;
        
        public GroupController(IGroupService groupService, IUserService userService)
        {
            _groupService = groupService;
            _userService = userService;
        }

        /// <summary>
        /// GET: file/browse
        /// </summary>
        /// <returns>A View showing the user's files</returns>
        [Route("{groupId}")]
        [HttpGet]
        public ActionResult GroupSettings(Guid groupId)
        {
            var group = _groupService.GetByGroupId(groupId);

            ViewBag.Group = group;
            ViewBag.User = _userService.GetFromHttpContext(HttpContext);
            
            return View();
        }

    }
}
