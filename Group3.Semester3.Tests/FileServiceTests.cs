using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Group3.Semester3.WebApp.Entities;
using Moq;
using Group3.Semester3.WebApp.Models.FileSystem;
using Group3.Semester3.WebApp.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Group3.Semester3.WebAppTests
{
    class FileServiceTests
    {
        private Helper _helper;
        private FileEntity _file;
        private Guid _testFileGuid;
        private string _testAzureName;
        private Guid _testParentId;
        private UserModel _userModel;
        
        [SetUp]
        public void Setup()
        {
            _helper = new Helper();

            _testParentId = Guid.NewGuid();
            _testFileGuid = Guid.NewGuid();
            _testAzureName = Guid.NewGuid().ToString();
            _file = new FileEntity()
            {
                Id = _testFileGuid,
                AzureName = _testAzureName,
                Name = "test",
                ParentId = _testParentId
            };
            
            _userModel = new UserModel { Id = Guid.NewGuid() };
            
            _helper.MockedAccessService.Setup(
                    s => s.HasAccessToFile(null, It.IsAny<FileEntity>(), It.IsAny<int>()))
                .Verifiable();
            _helper.MockedAccessService.Setup(
                    s => s.HasAccessToFile(_userModel, It.IsAny<FileEntity>(), It.IsAny<int>()))
                .Verifiable();
            
            _helper.MockedFileRepository.Setup(s => s.GetById(_testFileGuid)).Returns(_file);
            
            _helper.MockedFileRepository.Setup(s => s.Delete(_testFileGuid)).Returns(true);
            _helper.MockedFileRepository.Setup(s => s.Update(It.IsAny<FileEntity>())).Returns(true);
            _helper.MockedFileRepository.Setup(s => s.Insert(It.IsAny<FileEntity>())).Returns(true);

            _helper.MockedFileRepository.Setup(s =>
                s.GetFile(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), _file.Name)).Returns(_file);
            _helper.MockedFileRepository.Setup(s => s.MoveIntoFolder(_testFileGuid, _file.ParentId)).Returns(true);
            
            _helper.MockedFileRepository.Setup(s => s.UpdateFileAndCreateNewVersion(It.IsAny<FileEntity>(), It.IsAny<FileVersion>()))
                .Returns(true);
            
            
            _helper.MockedAzureService.Setup(s => s.DeleteFileAsync(_testAzureName)).Verifiable();
            _helper.MockedAzureService.Setup(s => s.GenerateDownloadLink(_testAzureName, "test")).Returns(_testAzureName);
            _helper.MockedAzureService.Setup(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>())).Verifiable();
        }

        [Test]
        public void Test1()
        {
            var fileService = _helper.GetFileService();
            if (fileService == null)
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestUploadNewFile()
        {
            var fileService = _helper.GetFileService();

            var uploadFiles = new List<IFormFile>();
            
            var file = new FormFile(new MemoryStream(), 0,10,"test-new", "test-new");
            
            uploadFiles.Add(file);
            
            var result = fileService.UploadFile(_userModel, "", "", uploadFiles);

            Assert.DoesNotThrow(() =>
            {
                var fileResult = result.Result.Find(entry => entry.Name == file.Name);
                Assert.AreEqual(file.Name, fileResult.Name);
            });
            
            _helper.MockedAzureService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _helper.MockedFileRepository.Verify(s => s.Insert(It.IsAny<FileEntity>()), Times.Once);
        }

        [Test]
        public void TestUploadExistingFile()
        {
            var fileService = _helper.GetFileService();

            var uploadFiles = new List<IFormFile>();
            
            var file = new FormFile(new MemoryStream(), 0,10,"test", "test");
            
            uploadFiles.Add(file);
            
            var result = fileService.UploadFile(_userModel, "", "", uploadFiles);

            // Existing files should not be added to result list
            Assert.IsEmpty(result.Result);
            
            _helper.MockedAzureService.Verify(s => s.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _helper.MockedFileRepository.Verify(s => 
                s.UpdateFileAndCreateNewVersion(It.IsAny<FileEntity>(), It.IsAny<FileVersion>()), Times.Once);
        }

        [Test]
        public void TestDownload()
        {
            var fileService = _helper.GetFileService();

            var (fileEntity, link) = fileService.DownloadFile(_testFileGuid, "", null);
            
            Assert.AreEqual(_file.AzureName, link);
            
            Assert.AreEqual(_file.Id, fileEntity.Id);
        }

        [Test]
        public void TestRename()
        {
            var fileService = _helper.GetFileService();

            var renameFile = fileService.RenameFile(_testFileGuid, null, "test-renamed");
            
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Exactly(2));
            _helper.MockedFileRepository.Verify(s => s.Update(renameFile), Times.Once);
            
            Assert.AreEqual("test-renamed", renameFile.Name);
        }

        [Test]
        public void TestDeleteFile()
        {
            var fileService = _helper.GetFileService();
            
            var result = fileService.DeleteFile(_testFileGuid, null);
            
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Exactly(2));
            _helper.MockedFileRepository.Verify(s => s.Delete(_testFileGuid), Times.Once);
            _helper.MockedAzureService.Verify(s => s.DeleteFileAsync(_testAzureName), Times.Once);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestDeleteFolder()
        {
            var fileService = _helper.GetFileService();
            
            _file.IsFolder = true;

            var result = fileService.DeleteFile(_testFileGuid, null);
            
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Once);
            _helper.MockedFileRepository.Verify(s => s.Delete(_testFileGuid), Times.Once);
            _helper.MockedAzureService.Verify(s => s.DeleteFileAsync(_testAzureName), Times.Never);
            
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestGenerateDownloadLink()
        {
            var fileService = _helper.GetFileService();

            var downloadLink = fileService.GenerateDownloadLink(_file, null);

            _helper.MockedAzureService.Verify(s => s.GenerateDownloadLink(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));

            Assert.AreEqual(downloadLink, _file.AzureName);
        }

        [Test]
        public void TestCreateFolder()
        {
            var userModel = new UserModel { Id = Guid.NewGuid() };
            _helper.MockedAccessService.Setup(
                    s => s.HasAccessToFile(userModel, It.IsAny<FileEntity>(), It.IsAny<int>()))
                .Verifiable();

            var fileService = _helper.GetFileService();
            var createFolderModel = new CreateFolderModel { Name = "folder" };

            var folder = fileService.CreateFolder(userModel, createFolderModel);
            

            _helper.MockedFileRepository.Verify(s => s.Insert(It.IsAny<FileEntity>()), Times.Exactly(1));
            _helper.MockedAccessService.Verify(s => s.HasAccessToFile(null, It.IsAny<FileEntity>(), It.IsAny<int>()), Times.Never);
            Assert.AreEqual(createFolderModel.Name, folder.Name);
        }

        [Test]
        public void TestGetById()
        {
            var fileService = _helper.GetFileService();

            var returnedFile = fileService.GetById(_testFileGuid);

            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Once);

            Assert.AreEqual(_testFileGuid, returnedFile.Id);
        }

        [Test]
        public void TestMoveIntoFolder()
        {
            var fileService = _helper.GetFileService();

            var isMoved = fileService.MoveIntoFolder(_file, null);

            _helper.MockedAccessService.Verify(
                    s => s.HasAccessToFile(null, It.IsAny<FileEntity>(), It.IsAny<int>()), Times.Once);
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Once);
            _helper.MockedFileRepository.Verify(s => s.MoveIntoFolder(_testFileGuid, _file.ParentId), Times.Once);

            Assert.IsTrue(isMoved);
        }
    }
}
