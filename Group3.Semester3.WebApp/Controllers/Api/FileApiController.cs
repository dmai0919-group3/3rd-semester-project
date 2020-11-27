using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.FileSystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Group3.Semester3.WebApp.Controllers.Api
{
    [Route("api/file")]
    [ApiController]
    [Authorize]
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
            var user = _userService.GetFromHttpContext(HttpContext);
            var fileEntities = _fileService.BrowseFiles(user, parentId);
            return Ok(fileEntities);
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
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string parentGuid)
        {
           
            try
            {
                List<Models.FileSystem.FileEntry> generatedEntries = await _fileService.UploadFile(
                    _userService.GetFromHttpContext(HttpContext),
                    parentGuid,
                    files);
                return Ok(generatedEntries);
            } catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        // PUT api/<FileApiController>/5

        [HttpPut("{id}")]
        public IActionResult UpdateFile(Guid fileId, [FromBody] string name)
        {
            var user = _userService.GetFromHttpContext(HttpContext);
            var file = _fileService.RenameFile(fileId, user.Id, name);
            if (file != null)
            {
                return Ok(file);
            }
            else return NoContent();
        }

        // DELETE api/<FileApiController>/5
        [HttpDelete("{id}")]
        public IActionResult DeleteFile(Guid fileId)
        {
            var user = _userService.GetFromHttpContext(HttpContext);
            var result = _fileService.DeleteFile(fileId, user.Id);
            if(!result)
            {
                return BadRequest();
            }
            else return NoContent();
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
