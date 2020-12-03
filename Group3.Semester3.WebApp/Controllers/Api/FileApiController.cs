using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        // GET: api/file/browse
        [HttpGet]
        [Route("browse")]
        public IActionResult GetFiles()
        {
            var user = _userService.GetFromHttpContext(HttpContext);
            var fileEntities = _fileService.BrowseFiles(user, "0");
            return Ok(fileEntities);
        }

        // GET: api/file/browse/{guid}
        [HttpGet]
        [Route("browse/{parentId}")]
        public IActionResult GetFiles(string parentId)
        {
            try
            {
                var user = _userService.GetFromHttpContext(HttpContext);
                var fileEntities = _fileService.BrowseFiles(user, parentId);
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

        // GET api/file/5
        [HttpGet("{id}")]
        public string GetFile(int id)
        {
            // TODO: Return certain file information (only if user has access to the file)
            return "value";
        }

        // POST api/<FileApiController>
        [HttpPost]
        [Route("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFiles([FromForm] UploadFilesModel model)
        {
            try
            {
                List<Models.FileSystem.FileEntry> generatedEntries = await _fileService.UploadFile(
                    _userService.GetFromHttpContext(HttpContext),
                    model.ParentId,
                    model.Files);
                return Ok(generatedEntries);
            } catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

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
    }
}
