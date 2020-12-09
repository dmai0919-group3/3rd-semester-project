using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;
using Group3.Semester3.WebApp.Models.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Group3.Semester3.WebApp.Controllers.Api
{
    [Route("api/file")]
    [ApiController]
    [Authorize(AuthenticationSchemes = (CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme))]
    public class FileApiController : ControllerBase
    {
        private IFileService _fileService;
        private IUserService _userService;

        public FileApiController(IFileService fileService, IUserService userService)
        {
            _fileService = fileService;
            _userService = userService;
        }

        /// <summary>
        /// GET: api/file/browse
        /// </summary>
        /// <returns>FileEntities that are owned by the user</returns>
        [HttpGet]
        [Route("browse")]
        public IActionResult BrowseFiles([FromQuery] string groupId, [FromQuery] string parentId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var fileEntities = _fileService.BrowseFiles(user, groupId, parentId);
                return Ok(fileEntities);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// GET: api/file/{id}
        /// TODO Return certain file information (only if user has access to the file)
        /// </summary>
        /// <param name="id">The id of a file</param>
        /// <returns>The information of a file (if the user has access to it)</returns>
        [HttpGet("{id}")]
        public string GetFile(int id)
        {
            return "NotImplemented";
        }

        /// <summary>
        /// GET: api/download/{fileId}
        /// Returns a FileEntity and a download URL from the Azure Blob Storage.
        /// The returned downloadLink is valid for 24 hours from the moment the request is sent.
        /// </summary>
        /// <param name="fileId">The ID of the file we want to download</param>
        /// <returns>200 Ok((FileEntity file, string downloadLink)) if the request was successful or 400 BadRequest with the Exception's message if the request failed.</returns>
        [HttpGet]
        [Route("download/{fileId}")]
        public IActionResult downloadFile(Guid fileId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.DownloadFile(fileId, user);

                FileEntity file = result.Item1;
                string downloadLink = result.Item2; 

                return Ok(new {file, downloadLink});
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// POST: api/file/upload
        /// </summary>
        /// <param name="model">UploadFilesModel containing a file which then gets uploaded</param>
        /// <returns>A list of FileEntries of the newly uploaded files</returns>
        [HttpPost]
        [Route("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFiles([FromForm] UploadFilesModel model)
        {
            try
            {
                List<Models.FileSystem.FileEntry> generatedEntries = await _fileService.UploadFile(
                    _userService.GetFromHttpContext(HttpContext),
                    model.GroupId,
                    model.ParentId,
                    model.Files);
                return Ok(generatedEntries);
            } catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        /// <summary>
        /// DELETE: api/file/delete
        /// </summary>
        /// <param name="model">The FileEntity that needs to be deleted</param>
        /// <returns>204 NoContent if the request was successful</returns>
        [Route("delete")]
        [HttpDelete]
        public ActionResult Delete(FileEntity model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.DeleteFile(model.Id, user);
                if (!result)
                {
                    return BadRequest();
                }
                else return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// PUT: api/file/rename
        /// </summary>
        /// <param name="model">The FileEntity with the new name</param>
        /// <returns>The FileEntity of the renamed file</returns>
        [Route("rename")]
        [HttpPut]
        public ActionResult Rename(FileEntity model)
        {
            try {
                var user = _userService.GetFromHttpContext(HttpContext);
                
                var result = _fileService.RenameFile(model.Id, user, model.Name);
                return Ok(result);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// POST: api/file/create-folder
        /// </summary>
        /// <param name="model">The CreateFolderModel containing the info of the new folder</param>
        /// <returns>The FileEntity object of the new folder</returns>
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
            catch (Exception exception)
            {
                return BadRequest("System error, please contact Administrator");
            }
        }

        /// <summary>
        /// POST: api/file/move
        /// </summary>
        /// <param name="model">A FileEntity object with the new parent details</param>
        /// <returns>True if successful</returns>
        [Route("move")]
        [HttpPost]
        public ActionResult MoveIntoFolder(FileEntity model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var result = _fileService.MoveIntoFolder(model, user);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("browse-shared")]
        public IActionResult BrowseSharedFiles()
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var fileEntities = _fileService.BrowseSharedFiles(user);
                return Ok(fileEntities);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("get-shared-with")]
        public IActionResult GetSharedWithUsers(FileEntity fileEntity)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var fileEntities = _fileService.SharedWithList(fileEntity, user);
                return Ok(fileEntities);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [Route("share")]
        [HttpPost]
        public ActionResult ShareFile(FileEntity fileEntity)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var hash = _fileService.ShareFile(fileEntity, user);

                var url = Url.Action("SharedFileLink", "File", new {hash = hash},  Request.Scheme);
                
                return Ok(url);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception e)
            {
                return BadRequest("System error, please contact administrator.");
            }
        }
        
        [Route("share-with")]
        [HttpPost]
        public ActionResult ShareFileWith(SharedFile sharedFile)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.ShareFile(sharedFile, user);
                return Ok(result);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception e)
            {
                return BadRequest("System error, please contact administrator.");
            }
        }

        [Route("share")]
        [HttpDelete]
        public ActionResult UnShareFile(SharedFile sharedFileToDelete)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.UnShareFile(sharedFileToDelete, user);
                if (!result)
                {
                    return BadRequest();
                }
                else return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest("System error, please contact administrator.");
            }
        }

        /// <summary>
        /// GET: api/file/content/{id}
        /// </summary>
        /// <param name="id">The id of a file</param>
        /// <returns>An UpdateFileModel containing the contents of the file matching the given id</returns>
        [Route("content/{id}")]
        [HttpGet]
        public ActionResult GetFileContents(string id)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var updateFileModel = _fileService.GetFileContents(id, user);
                
                return Ok(updateFileModel);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                return BadRequest("System error, please contact Administrator");
            }
        }

        /// <summary>
        /// POST: api/file/content
        /// </summary>
        /// <param name="model">An UpdateFileModel containing the new contents of a file</param>
        /// <returns>The new FileEntity object</returns>
        [Route("content")]
        [HttpPost]
        public IActionResult SetFileContents([FromBody] UpdateFileModel model)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var file = _fileService.UpdateFileContents(model, user);

                return Ok(file);
            }
            catch (ConcurrencyException exception)
            {
                return Conflict(exception.Message);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                return BadRequest("System error, please contact Administrator");
            }
        }
    }
}
