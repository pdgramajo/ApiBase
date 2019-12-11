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
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json.Converters;

namespace api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FileController : ControllerBase
    {
        // By default, WebApi serializes enum's as integers. This tells it to use strings instead
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductType
        {
            rr = 2,
            aa = 3,
            bb = 4
        }

        /// <summary>Upload an avatar</summary>
        /// <remarks> we are testing this</remarks>
        /// POST /api/File
        [HttpPost, DisableRequestSizeLimit]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Upload(IFormFile file, ProductType TimeBasis)
        {
            try
            {
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
                    return Ok(new
                    {
                        absolutPath = baseURL + "/" + dbPath.Replace("\\", "/"),
                        relativePath = dbPath.Replace("\\", "/")
                    });
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Delete(string id)
        {
            try
            {
                var folderName = CUtils.GetFolderPathToSave(CUtils.File);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var fullPath = Path.Combine(pathToSave, id);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                else
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error " + ex.Message);
            }

        }
    }
}