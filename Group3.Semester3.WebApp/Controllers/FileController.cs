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
            var user = _userService.GetFromHttpContext(HttpContext);
            var fileEntities = _fileService.BrowseFiles(user);
            ViewBag.Files = fileEntities;

            return View();
        }

        [Route("delete")]
        [HttpDelete]
        public ActionResult Delete(Guid guid)
        {
            var result = _fileService.DeleteFile(guid);

            return View();
        }

        [Route("rename")]
        [HttpPut]
        public ActionResult Rename(Guid guid, string name)
        {
            var result = _fileService.RenameFile(guid, name);

            return View();
        }



    }
}
