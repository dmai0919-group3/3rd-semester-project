using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Group3.Semester3.WebApp.Helpers;
using Microsoft.Extensions.Configuration;

namespace Group3.Semester3.WebApp.BusinessLayer
{

    public interface IAzureService
    {
        public Task<Response<BlobContentInfo>> UploadBlobAsync(string blobName, Stream fileContent);

        public Task<Response> DeleteFileAsync(string blobName);

        public string GenerateDownloadLink(string blobName, string fileName);

        public Stream GetFileContents(string blobName);

        public void UploadFileContents(string blobName, Stream fileContentStream);

        public long GetFileSize(string blobName);
    }
    
    public class AzureService : IAzureService
    {
        private BlobContainerClient _containerClient;
        private IConfiguration _configuration;

        public AzureService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            _containerClient = new BlobContainerClient(
                _configuration.GetConnectionString("AzureConnectionString"),
                _configuration.GetSection("AppSettings").Get<AppSettings>().AzureDefaultContainer);
            _containerClient.CreateIfNotExists();
        }


        public async Task<Response<BlobContentInfo>> UploadBlobAsync(string blobName, Stream fileContent)
        {
            return await _containerClient.UploadBlobAsync(blobName, fileContent);
        }

        public async Task<Response> DeleteFileAsync(string blobName)
        {
            return await _containerClient.GetBlobClient(blobName).DeleteAsync();
        }

        public string GenerateDownloadLink(string blobName, string fileName)
        {
            var blobSasBuilder = new BlobSasBuilder()
            {
                StartsOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddHours(24),
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ContentDisposition = new ContentDisposition()
                {
                    FileName = fileName
                }.ToString()
            };

            blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            var storageSharedKeyCredential = new StorageSharedKeyCredential(
                _configuration.GetSection("AppSettings").Get<AppSettings>().AzureStorageAccount,
                _configuration.GetSection("AppSettings").Get<AppSettings>().AzureAccountKey);

            var sasToken = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();

            return $"{_containerClient.GetBlockBlobClient(blobName).Uri}?{sasToken}";
        }

        public Stream GetFileContents(string blobName)
        {
            var response = _containerClient.GetBlobClient(blobName).Download();
            
            return response.Value.Content;
        }

        public void UploadFileContents(string blobName, Stream fileContentStream)
        {
            _containerClient.GetBlobClient(blobName).Upload(fileContentStream, true);
        }

        public long GetFileSize(string blobName)
        {
            return _containerClient.GetBlobClient(blobName).GetProperties().Value.ContentLength;
        }
    }
}