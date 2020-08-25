using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStore_API.Contracts;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _config;
        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILoggerService logger, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }
        /// <summary>
        /// User Login Endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetCollectActionNames();
            try
            {
                var username = userDTO.Username;
                var Password = userDTO.Password;
                _logger.LogInfo($"{location}:login attempt for user:{username}");
                var result = await _signInManager.PasswordSignInAsync(username, Password, false, false);
                if (result.Succeeded)
                {
                    _logger.LogInfo($"{location}:{username} successful authanticated");
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenstring = await GenerateJSONWebToken(user);
                    return Ok(new { token=tokenstring});
                }
                _logger.LogInfo($"{location}:{username} authantication failed");
                return Unauthorized(userDTO);
            }
            catch(Exception ex)
            {
                return InternalError($"{location}-{ex.Message}-{ex.InnerException}");
            }
        }
        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Key"]));
            var Credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var Claims = new List<Claim>
            { 
                new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            Claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultNameClaimType, r)));
            var token = new JwtSecurityToken(
                _config["jwt:Issuer"],
                _config["jwt:Issuer"],
                Claims,
                null,
                expires:DateTime.Now.AddMinutes(5),
                signingCredentials:Credential
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GetCollectActionNames()
        {
            var controllerName = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controllerName}-{action}";

        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong,Please contact the Adminstrator");
        }
    }
}
