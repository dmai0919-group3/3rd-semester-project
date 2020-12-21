using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Groups;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Group3.Semester3.WebApp.Helpers;

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
                return BadRequest(Messages.SystemError);
            }
        }

        [Route("delete")]
        [HttpPost]
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
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
                return BadRequest(Messages.SystemError);
            }
        }

        [Route("add-user")]
        [HttpPost]
        public ActionResult AddUser(AddUserGroupModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.AddUser(user, model);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }
        
        [Route("change-permissions")]
        [HttpPut]
        public ActionResult ChangeUserPermissions(AddUserGroupModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.UpdateUserPermissions(user, model);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }

        [Route("get-users")]
        [HttpGet]
        public ActionResult GetGroupUsers([FromQuery] Guid groupId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.GetGroupUsers(user, groupId);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }
        
        [Route("get-permissions")]
        [HttpGet]
        public ActionResult GetGroupUser([FromQuery] string groupId, [FromQuery] string userId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _groupService.GetUser(user, groupId, userId);

                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }
    }
}
