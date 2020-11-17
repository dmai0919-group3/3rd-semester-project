using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Group3.Semester3.WebApp.Helpers;
using System;

namespace Group3.Semester3.WebApp.Controllers
{
    [Route("file")]
    public class FileController : Controller
    {

        private IConfiguration _configuration;

        public FileController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        public async Task<IActionResult> Upload(List<IFormFile> files, string toAzure)
        {
            long size = files.Sum(f => f.Length);

            List<string> fileNames = new List<string>();

            if (toAzure == "1")
            {

                BlobContainerClient containerClient =
                    new BlobContainerClient(
                        _configuration.GetConnectionString("AzureConnectionString"),
                        _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);

                containerClient.CreateIfNotExists();

                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var blobName = Guid.NewGuid().ToString();
                        fileNames.Add(blobName);
                        await containerClient.UploadBlobAsync(blobName, formFile.OpenReadStream());
                    }
                }
            }
            else
            {
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var filePath = System.IO.Path.GetTempFileName();
                        fileNames.Add(filePath);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }
            }

            return Ok(new
            {
                message = "Nr. of files uploaded: " + files.Count.ToString(),
                filenames = fileNames,
                toAzure
            });
        }
    }
}
