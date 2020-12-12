using System;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
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
        private IUserService _userService;
        private ICommentService _commentService;

        public CommentApiController(IUserService userService, ICommentService commentService)
        {
            _userService = userService;
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] string fileId, [FromQuery] string parentId)
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
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        private Guid ParseGuid(string guid)
        {
            Guid parsedGuid = Guid.Empty;

            try
            {
                parsedGuid = System.Guid.Parse(guid);
            }
            catch { }

            return parsedGuid;
        }

    }
}