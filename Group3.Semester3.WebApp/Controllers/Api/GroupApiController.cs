using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Groups;
using Group3.Semester3.WebApp.Models.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    [Authorize(AuthenticationSchemes = (CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme))]
    public class GroupApiController : ControllerBase
    {
        private IGroupService _groupService;
        private IUserService _userService;
        public GroupApiController(IGroupService groupService, IUserService userService)
        {
            _groupService = groupService;
            _userService = userService;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.GetUserGroups(user);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest("System error, please contact Administrator");
            }
        }

        [Route("delete")]
        [HttpDelete]
        public ActionResult Delete(Group model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.DeleteGroup(model.Id, user);
                if (!result)
                {
                    return BadRequest();
                }
                else return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("rename")]
        [HttpPut]
        public ActionResult Rename(Group model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var result = _groupService.RenameGroup(model.Id, user, model.Name);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("create-group")]
        [HttpPost]
        public ActionResult CreateGroup(CreateGroupModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.CreateGroup(user, model);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest("System error, please contact Administrator");
            }
        }

        [Route("add-user")]
        [HttpPost]
        public ActionResult AddUser(UserGroupModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.AddUser(user.Email, model);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest("System error, please contact Administrator");
            }
        }

        [Route("remove-user")]
        [HttpDelete]
        public ActionResult RemoveUser(UserGroupModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.RemoveUser(user, model);
                if (!result)
                {
                    return BadRequest();
                }
                else return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
