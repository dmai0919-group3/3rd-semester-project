using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Group3.Semester3.WebApp.BusinessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Entities;
using System;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("file")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class FileController : Controller
    {

        private IFileService _fileService;
        private IUserService _userService;

        public FileController(IFileService fileService, IUserService userService)
        {
            _fileService = fileService;
            _userService = userService;
        }

        [Route("upload")]
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [Route("upload")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(List<IFormFile> files, string parentGuid)
        {

            List<Models.FileSystem.FileEntry> generatedEntries = await _fileService.UploadFile(
                    _userService.GetFromHttpContext(HttpContext), 
                    parentGuid, 
                    files);

            // TODO compare with input, generate errors

            foreach (var file in generatedEntries)
            {
                var message = "File " + file.Name + " successfully uploaded";
                Messenger.addMessage(message);
            }

            return RedirectToAction("Upload");
        }

        [Route("browse")]
        [HttpGet]
        public ActionResult Browse()
        {
            return View();
        }

        [Route("create-folder")]
        [HttpPost]
        public ActionResult CreateFolder(CreateFolderModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.CreateFolder(user, model);

                return Ok(result);
            } 
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest("System error, please contact Administrator");
            }
        }

    }
}
