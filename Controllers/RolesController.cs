using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using Microsoft.EntityFrameworkCore;
using ApiBase.Models;
using ApiBase.Helpers;
using Newtonsoft.Json;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleMgr, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleMgr;
            _userManager = userManager;
        }

        // GET: api/Role
        [HttpGet]
        public ActionResult<IQueryable<IdentityRole>> Get([FromQuery] RoleParameters roleParameters)
        {

            var roles = _roleManager.Roles;
            
            var result = PagedList<IdentityRole>.ToPagedList(roles, roleParameters.PageNumber, roleParameters.PageSize);

            var metadata = new
            {
                result.TotalCount,
                result.PageSize,
                result.CurrentPage,
                result.TotalPages,
                result.HasNext,
                result.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(result);
        }

        // GET: api/Role/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<IdentityRole>> Get(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (string.IsNullOrEmpty(role.Id))
            {
                return BadRequest("el id no existe");
            }

            return role;
        }
        // GET: api/Role/5
        [HttpGet("{id}/Users")]
        public async Task<ActionResult<IQueryable<ApplicationUserBasicData>>> GetUsersByRoleId(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (string.IsNullOrEmpty(role.Id))
            {
                return BadRequest("el id no existe");
            }

            var users = await _userManager.Users
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
                        }).ToListAsync();

            var usersFiltered = users.Where(user => user.Roles == role.Name);

            return Ok(usersFiltered);
        }

        // POST: api/Role
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IdentityRole>> Post([FromBody] RoleInfo rol)
        {
            var role = new IdentityRole(rol.Name);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return Ok(role);
            }
            else
            {
                return BadRequest("error al crear el rol: " + result);
            }
        }

        // PUT: api/Role/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IdentityRole>> Put(string id, [FromBody] RoleInfo roleInfo)
        {
            if (string.IsNullOrEmpty(roleInfo.Name) || string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            bool exist = await _roleManager.RoleExistsAsync(roleInfo.Name);

            if (exist)
            {
                BadRequest("el role name ya existe");
            }

            var rol = await _roleManager.FindByIdAsync(id);
            rol.Name = roleInfo.Name;

            var result = await _roleManager.UpdateAsync(rol);

            if (result.Succeeded)
            {
                return Ok(rol);
            }
            else
            {
                return BadRequest("error al editar el rol");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IdentityRole>> Delete(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            var result = await _roleManager.DeleteAsync(rol);

            if (result.Succeeded)
            {
                return Ok(rol);
            }
            else
            {
                return BadRequest("error al eliminar el rol");
            }
        }
    }
}