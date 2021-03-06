using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleMgr, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleMgr;
            _configuration = configuration;
        }

        /// <summary>To add a user the user logged need be Admin or Manager</summary>
        /// POST api/Accounts/Create
        [Route("Create")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateUser([FromBody] UserInfoCreate model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, AvatarUrl = model.AvatarUrl };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!string.IsNullOrEmpty(model.RoleName))
                {
                    // var role = await _roleManager.FindByIdAsync(model.RoleId);

                    await _userManager.AddToRoleAsync(user, model.RoleName);
                }

                if (result.Succeeded)
                {
                    return await BuildToken(model);
                }
                else
                {
                    return BadRequest("Username or Password invalid");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>Get the token to login in the API</summary>
        /// POST api/Accounts/Login
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserInfo model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user.Enabled == false)
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login attempt.");
                    return BadRequest(ModelState);
                }
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, true, false);
                if (result.Succeeded)
                {
                    return await BuildToken(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login attempt.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private async Task<IActionResult> BuildToken(UserInfo model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var roles = await _userManager.GetRolesAsync(user);

            var secretKey = _configuration["SuperSecretKey"]; //esto esta en una variable de ambiente
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // var expiration = DateTime.UtcNow.AddHours(8);

            // var header = new JwtHeader(cred);

            // var payload = new JwtPayload {
            //     {"params", new { }},// this is for future parameters
            //     {JwtRegisteredClaimNames.Exp, expiration},
            //     {JwtRegisteredClaimNames.Sub,user.Id},
            //     {"name",user.Email},
            //     {JwtRegisteredClaimNames.Email,user.Email},
            //     {"roles", string.Join(",",roles)},
            //     {JwtRegisteredClaimNames.Iss, "yourdomain.com"},
            //     {JwtRegisteredClaimNames.Aud, "yourdomain.com"}
            // };

            // var token = new JwtSecurityToken(header, payload);

            var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                            new Claim(JwtRegisteredClaimNames.Email,user.Email),
                            new Claim("name",user.Email),
                            new Claim("avatar",user.AvatarUrl),
                            new Claim("roles", string.Join(",",roles)),
                        };



            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(120);
            var token = new JwtSecurityToken(_configuration["Issuer"], _configuration["Audience"], claims, expires: expires, signingCredentials: creds);
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires
            });
        }
    }
}