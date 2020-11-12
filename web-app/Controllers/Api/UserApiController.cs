using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using web_app.Entities;
using web_app.Helpers;
using web_app.Models.Users;
using web_app.Services;

namespace web_app.Controllers.Api
{
    [Route("api/User")]
    [ApiController]
    public class UserApiController : ControllerBase
    {

        private IUserService _userService;
        private readonly AppSettings _appSettings;

        public UserApiController(
            IUserService userService,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        // POST api/<UserController>
        [Route("login")]
        [HttpPost]
        public IActionResult Login(AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Email, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // TODO: Change to ID later
                    new Claim(ClaimTypes.Name, user.Email.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                Token = tokenString
            });
        }

        // POST api/<UserController
        [Route("register")]
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            User user = new User();

            user.Email = model.Email;

            try
            {
                // create user
                _userService.Create(user, model.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
