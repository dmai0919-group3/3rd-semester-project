using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Group3.Semester3.WebApp.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using Group3.Semester3.WebApp.BusinessLayer;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("file")]
    [Authorize]
    public class FileController : Controller
    {

        private IConfiguration _configuration;
        private IFileService _fileService;
        private IUserService _userService;

        public FileController(IConfiguration configuration, IFileService fileService, IUserService userService)
        {
            _configuration = configuration;
            _fileService = fileService;
            _userService = userService;
        }

        [Route("upload")]
        [HttpGet]
        [Authorize]
        public ActionResult Upload()
        {
            return View();
        }

        [Route("upload")]
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(List<IFormFile> files, string parentGuid)
        {
            List<Models.FileSystem.FileEntry> generatedEntries = await _fileService.UploadFile(
                    _userService.GetFromHttpContext(HttpContext), 
                    System.Guid.Parse(parentGuid), 
                    files);

            // TODO compare with input, generate errors

            return Ok(generatedEntries);
        }
    }
}
