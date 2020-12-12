using System;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
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

        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] Guid fileId, [FromQuery] Guid parentId)
        {
            // TODO: Get from service
            return Ok();
        }
        
    }
}