using System;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3.Semester3.WebApp.Controllers.Api
{
    [Route("api/file")]
    [ApiController]
    [Authorize(AuthenticationSchemes = (CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme))]
    public class FileApiController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IUserService _userService;

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
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }

        /// <summary>
        /// GET: api/download/{fileId}
        /// Returns a FileEntity and a download URL from the Azure Blob Storage.
        /// The returned downloadLink is valid for 24 hours from the moment the request is sent.
        /// </summary>
        /// <param name="fileId">The ID of the file we want to download</param>
        /// <param name="versionId">If set, download link will point to a previous version of a file</param>
        /// <returns>200 Ok((FileEntity file, string downloadLink)) if the request was successful or 400 BadRequest with the Exception's message if the request failed.</returns>
        [HttpGet]
        [Route("download/{fileId}")]
        public IActionResult DownloadFile(Guid fileId, [FromQuery] string versionId = "")
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.DownloadFile(fileId, versionId, user);

                var file = result.Item1;
                var downloadLink = result.Item2; 

                return Ok(new {file, downloadLink});
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
                var generatedEntries = await _fileService.UploadFile(
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
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }
        
        [HttpGet]
        [Route("share/{fileId}")]
        public IActionResult GetShareInfo(Guid fileId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var fileEntity = _fileService.GetById(fileId);

                var shareInfo = _fileService.GetShareInfo(fileEntity, user);
                
                var url = Url.Action("SharedFileLink", "File", new {hash = shareInfo.Item2},  Request.Scheme);
                var users = shareInfo.Item1;

                return Ok(new {Link = url, Users = users});
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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

                var url = Url.Action("SharedFileLink", "File", new {hash},  Request.Scheme);
                
                return Ok(url);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch
            {
                return BadRequest(Messages.SystemError);
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
                    return NotFound();
                }
                else return NoContent();
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }
        
        [Route("disable-share-link")]
        [HttpPost]
        public ActionResult DisableShareLink(FileEntity fileEntity)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.DisableShareLink(fileEntity, user);
                if (!result)
                {
                    return NotFound();
                }
                else return NoContent();
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }
        
        [Route("disable-sharing")]
        [HttpPost]
        public ActionResult DisableSharing(FileEntity fileEntity)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var result = _fileService.DisableSharing(fileEntity, user);
                if (!result)
                {
                    return NotFound();
                }
                else return NoContent();
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch
            {
                return BadRequest(Messages.SystemError);
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
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }

        [Route("versions")]
        [HttpGet]
        public IActionResult GetFileVersions([FromQuery] Guid fileId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var versions = _fileService.GetFileVersions(fileId, user);
                
                return Ok(versions);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch
            {
                return BadRequest(Messages.SystemError);
            }
        }

        [Route("revert-version")]
        [HttpPost]
        public IActionResult RevertFile(FileVersion fileVersion)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var version = _fileService.RevertFileVersion(fileVersion, user);
                
                return Ok(version);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch 
            {
                return BadRequest(Messages.SystemError);
            }
        }

        [Route("get-path/{fileId}")]
        [HttpGet]
        public IActionResult GetFilePath(Guid fileId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);

                var path = _fileService.GetParents(fileId, user);
                
                return Ok(path);
            }
            catch (ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch 
            {
                return BadRequest(Messages.SystemError);
            }
        }
    }
}
