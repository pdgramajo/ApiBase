using System;
using System.Net;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using api.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ApiBase.Helpers;
using Newtonsoft.Json;

namespace api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleMgr, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleMgr;
            _configuration = configuration;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IQueryable<ApplicationUserBasicData>>> Get([FromQuery] UserParameters userParameters)
        {
            var users = await _userManager.Users
            .Where(s => s.Enabled == true)
            .Select(user => new ApplicationUserBasicData
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Roles = string.Join(",", user.Roles.Join(_roleManager.Roles,
                                            userRole => userRole.RoleId,
                                            role => role.Id,
                                            (userRole, role) => role.Name
                                            ).ToList())
            })
            .ToListAsync();
            var result = PagedList<ApplicationUserBasicData>.ToPagedList(users, userParameters.PageNumber, userParameters.PageSize);

            var metadata = new
            {
                result.TotalCount,
                result.PageSize,
                result.CurrentPage,
                result.TotalPages,
                result.HasNext,
                result.HasPrevious
            };

            Response.Headers.Add("x-pagination", JsonConvert.SerializeObject(metadata));

            return Ok(result);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUserBasicData>> Get(string id)
        {
            var userExist = _userManager.Users.Any(x => x.Id == id);
            if (!userExist)
            {
                return NotFound();
            }

            var userData = await _userManager.Users.Where(user => user.Id == id).Select(user => new ApplicationUserBasicData
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Roles = string.Join(',', user.Roles.Join(_roleManager.Roles,
                                            userRole => userRole.RoleId,
                                            role => role.Id,
                                            (userRole, role) => role.Name
                                            ).ToList())
            }).FirstOrDefaultAsync();

            return Ok(userData);
        }

        /// <summary>Get all roles for a particular user</summary>
        // /// <param name="id">this is a UserId</param>
        [HttpGet("{id}/Roles")]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<List<string>>> GetRolesByUser(string id)
        {
            var userExist = _userManager.Users.Any(x => x.Id == id);
            if (!userExist)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [HttpPost("{id}/Role")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> AddRole(string id, [FromBody] RoleId role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var rol = await _roleManager.FindByIdAsync(role.Id);
                    var user = await _userManager.FindByIdAsync(id);
                    var result = await _userManager.AddToRoleAsync(user, rol.Name);
                    if (result.Succeeded)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }

                }
                catch (System.Exception)
                {

                    return BadRequest();
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> editUser(string id, [FromBody]UserEditData user)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.Users.AnyAsync(x => x.Id == id);
                if (!userExist)
                {
                    return NotFound();
                }

                var userToEdit = await _userManager.FindByIdAsync(id);

                var roles = await _userManager.GetRolesAsync(userToEdit);
                await _userManager.RemoveFromRolesAsync(userToEdit, roles.ToArray());

                await _userManager.AddToRoleAsync(userToEdit, user.rolName);

                userToEdit.PhoneNumber = user.PhoneNumber;
                userToEdit.AvatarUrl = user.AvatarUrl;

                var result = await _userManager.UpdateAsync(userToEdit);

                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        roles = roles,
                        result = result
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        roles = roles,
                        result = result
                    });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Delete(string id)
        {
            var userExist = await _userManager.Users.AnyAsync(x => x.Id == id);
            if (!userExist)
            {
                return NotFound();
            }
            var userToEdit = await _userManager.FindByIdAsync(id);
            userToEdit.Enabled = false;

            var result = await _userManager.UpdateAsync(userToEdit);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = result
                });
            }
            else
            {
                return BadRequest(new
                {
                    result = result
                });
            }
        }
    }
}