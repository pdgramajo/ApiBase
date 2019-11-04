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
                var folderName = Path.Combine("Resources", "Files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var directoryInfo = new DirectoryInfo(folderName);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Replace(" ", "_");

                    var fileNameToSave = string.Concat(
                                 Path.GetFileNameWithoutExtension(fileName),
                                 DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                 Path.GetExtension(fileName)
                                 );

                    var fullPath = Path.Combine(pathToSave, fileNameToSave);
                    var dbPath = Path.Combine(folderName, fileNameToSave);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { Path = "/" + dbPath.Replace("\\", "/") });
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