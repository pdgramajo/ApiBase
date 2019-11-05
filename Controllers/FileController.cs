using Microsoft.VisualBasic.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using System.IO;
using System.Net.Http.Headers;
using System;
using api.Helpers;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FileController : ControllerBase
    {
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = CUtils.GetFolderPathToSave(CUtils.File);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileNameToSave = CUtils.GetFileNameFormated(file.FileName);
                    var fullPath = Path.Combine(pathToSave, fileNameToSave);
                    var dbPath = Path.Combine(folderName, fileNameToSave);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    var baseURL = Request.Scheme + "://" + Request.Host.Value;
                    return Ok(new { Path = baseURL + "/" + dbPath.Replace("\\", "/") });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error " + ex.Message);
            }
        }
    }

}