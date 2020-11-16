using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.BusinessLayer;

namespace Group3.Semester3.WebApp.Controllers.Api
{
    [Route("api/User")]
    [ApiController]
    public class UserApiController : ControllerBase
    {

        private IUserService _userService;

        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }

        // POST api/<UserController>
        [Route("login")]
        [HttpPost]
        public IActionResult Login(AuthenticateModel model)
        {
            try
            {
                var user = _userService.Login(model);
                return Ok(user);
            }
            catch (Exception exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        // POST api/<UserController
        [Route("register")]
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            try
            {
                // create user
                var user = _userService.Register(model);

                return Ok(user);
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
