﻿using NUnit.Framework;
using System;
using Group3.Semester3.WebApp.Entities;
using Moq;

namespace Group3.Semester3.WebAppTests
{
    class FileServiceTests
    {
        private Helper _helper;
        private FileEntity _file;
        private Guid _testFileGuid;
        private string _testAzureName;
        private Guid _testParentId;
        
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
            
            _helper.MockedAccessService.Setup(
                    s => s.HasAccessToFile(null, It.IsAny<FileEntity>(), It.IsAny<int>()))
                .Verifiable();
            
            _helper.MockedFileRepository.Setup(s => s.GetById(_testFileGuid)).Returns(_file);
            _helper.MockedFileRepository.Setup(s => s.Delete(_testFileGuid)).Returns(true);
            _helper.MockedFileRepository.Setup(s => s.Update(It.IsAny<FileEntity>())).Returns(true);
            _helper.MockedAzureService.Setup(s => s.DeleteFileAsync(_testAzureName)).Verifiable();
            _helper.MockedFileRepository.Setup(s => s.MoveIntoFolder(_testFileGuid, _file.ParentId)).Returns(true);
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
        public void TestRename()
        {
            var fileService = _helper.GetFileService();

            var renameFile = fileService.RenameFile(_testFileGuid, null, "test-renamed");
            
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Exactly(2));
            _helper.MockedFileRepository.Verify(s => s.Update(renameFile), Times.Once);
            
            Assert.AreEqual("test-renamed", renameFile.Name);
        }

        [Test]
        public void TestDelete()
        {
            var fileService = _helper.GetFileService();
            
            var result = fileService.DeleteFile(_testFileGuid, null);
            
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Exactly(2));
            _helper.MockedFileRepository.Verify(s => s.Delete(_testFileGuid), Times.Once);
            _helper.MockedAzureService.Verify(s => s.DeleteFileAsync(_testAzureName), Times.Once);

            Assert.AreEqual(true, result);

            // Test for folder deletion
            
            _file.IsFolder = true;

            result = fileService.DeleteFile(_testFileGuid, null);
            
            _helper.MockedFileRepository.Verify(s => s.GetById(_testFileGuid), Times.Exactly(3));
            _helper.MockedFileRepository.Verify(s => s.Delete(_testFileGuid), Times.Exactly(2));
            _helper.MockedAzureService.Verify(s => s.DeleteFileAsync(_testAzureName), Times.Once);
            
            Assert.AreEqual(true, result);
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
