using System;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3.Semester3.WebApp.Controllers.Api
{

    [Route("api/comment")]
    [ApiController]
    [Authorize(AuthenticationSchemes = (CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme))]
    public class CommentApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;

        public CommentApiController(IUserService userService, ICommentService commentService)
        {
            _userService = userService;
            _commentService = commentService;
        }

        [HttpGet]
        public IActionResult GetComments([FromQuery] string fileId, [FromQuery] string parentId)
        {

            try
            {
                var fileGuid = ParseGuid(fileId);
                var user = _userService.GetFromHttpContext(HttpContext);
                var comments = _commentService.GetComments(user, fileGuid);
                return Ok(comments);
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

        private Guid ParseGuid(string guid)
        {
            var parsedGuid = Guid.Empty;

            try
            {
                parsedGuid = Guid.Parse(guid);
            }
            catch
            {
            }

            return parsedGuid;
        }

    }
}