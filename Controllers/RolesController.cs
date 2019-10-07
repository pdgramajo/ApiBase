using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using api.Models;

namespace webapinetcorebase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleMgr)
        {
            _roleManager = roleMgr;
        }

        // GET: api/Role
        [HttpGet]
        public ActionResult<IQueryable<IdentityRole>> Get()
        {
            return Ok(_roleManager.Roles);
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

            var claims = User.Claims.ToList();

            bool isAdmin = claims.Any(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && x.Value.Contains("Admin"));
            if (isAdmin)
            {

                return role;
            }

            return Unauthorized();
        }

        // POST: api/Role
        [HttpPost]
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
                return BadRequest("error al crear el rol: " + result );
            }
        }

        // PUT: api/Role/5
        [HttpPut("{id}")]
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