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
using Microsoft.AspNetCore.Authorization;

namespace Group3.Semester3.WebApp.Controllers.Api
{
    // implementation of an user api controller for an api/User/... endpoints
    [Route("api/User")]
    [ApiController]
    [Authorize]
    public class UserApiController : ControllerBase
    {
        // defining a user service through interface to flawlessly access the db
        private IUserService _userService;
        private readonly AppSettings _appSettings;

        public UserApiController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        // POST api/<UserController>
        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        // endpoint for logging the user into the app, validating his credentials, creating a session token 
        public IActionResult Login(AuthenticateModel model)
        {
            try
            {
                var user = _userService.Login(model);

                // authentication successful, generate token

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                var loginResult = new LoginResultModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Token = tokenString
                };

                return Ok(loginResult);
            }
            catch (Exception exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        // POST api/<UserController
        [Route("register")]
        [HttpPost]
        [AllowAnonymous]
        // endpoint for registration, registerModel is passed to user Service where user is inserted into DB
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

        [Route("current")]
        [HttpGet]
        // endpoint to get current user by retrieving the info from the httpcontext
        public IActionResult Current()
        {
            var user = _userService.GetFromHttpContext(HttpContext);

            return Ok(user);
        }

    }
}
