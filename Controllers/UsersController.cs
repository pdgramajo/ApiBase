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
        public async Task<ActionResult<IQueryable<ApplicationUserBasicData>>> Get()
        {
            var users = await _userManager.Users.Select(user => new ApplicationUserBasicData
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = string.Join(",", user.Roles.Join(_roleManager.Roles,
                                            userRole => userRole.RoleId,
                                            role => role.Id,
                                            (userRole, role) => role.Name
                                            ).ToList())
            }).ToListAsync();

            return Ok(users);
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
                Roles = string.Join(',', user.Roles.Join(_roleManager.Roles,
                                            userRole => userRole.RoleId,
                                            role => role.Id,
                                            (userRole, role) => role.Name
                                            ).ToList())
            }).FirstOrDefaultAsync();

            return Ok(userData);
        }

        [HttpGet("{id}/Roles")]
        public async Task<ActionResult<IdentityRole>> GetRolesByUser(string id)
        {
            var userExist = _userManager.Users.Any(x => x.Id == id);
            if (!userExist)
            {
                return NotFound("El usuario no existe");
            }
            var user = await _userManager.FindByIdAsync(id);
            var x = await _userManager.GetRolesAsync(user);
            return Ok(x);
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
                foreach (string rol in user.roles)
                {
                    await _userManager.AddToRoleAsync(userToEdit, rol);
                }

                userToEdit.PhoneNumber = user.PhoneNumber;

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
    }
}