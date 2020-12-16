using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace Group3.Semester3.WebApp.Controllers.Api
{
    [Route("api/User")]
    [ApiController]
    [Authorize]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;

        public UserApiController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// POST: api/User/login
        /// </summary>
        /// <param name="model">An AuthenticateModel of a user</param>
        /// <returns>The LoginResultModel containing the token of the user</returns>
        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
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
                    Subject = new ClaimsIdentity(new []
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
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
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }

        /// <summary>
        /// POST: api/User/register
        /// </summary>
        /// <param name="model">The RegisterModel of a new user</param>
        /// <returns>The UserModel of the new user</returns>
        [Route("register")]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(RegisterModel model)
        {
            try
            {
                // create user
                var user = _userService.Register(model);

                return Ok(user);
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

        /// <summary>
        /// GET: api/User/current
        /// </summary>
        /// <returns>The UserModel of the user matching the token used</returns>
        [Route("current")]
        [HttpGet]
        public IActionResult Current()
        {
            var user = _userService.GetFromHttpContext(HttpContext);

            return Ok(user);
        }

        /// <summary>
        /// POST: api/User/update
        /// </summary>
        [Route("update")]
        [HttpPost]
        public ActionResult UpdateUser(UserUpdateModel userParam)
        {
            try
            {
                var currentUser = _userService.GetFromHttpContext(HttpContext);
                var result = _userService.Update(userParam, currentUser);
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
