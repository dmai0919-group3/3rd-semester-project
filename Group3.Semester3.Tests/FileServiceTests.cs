using Group3.Semester3.WebApp.BusinessLayer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Group3.Semester3.WebAppTests
{
    class FileServiceTests
    {
        IFileService fileService;
        Guid fileId = new Guid("ced12f9b810e4efba9a4094fddfc33d4");
        Guid userId = new Guid("10F24309-A32F-431F-AE8C-A7E342437221");
        [SetUp]
        public void Setup()
        {
            fileService = Helper.GetFileService();
        }

        [Test, Order(0)]
        public void Test1()
        {
            if (fileService == null)
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test, Order(1)]
        public void TestRename()
        {
            var oldFile = fileService.GetById(fileId);
            //var newFile = fileService.RenameFile(fileId, userId, "test");
            //Assert.AreEqual("test", newFile.Name);
        }

        [Test, Order(2)]
        public void TestDelete()
        {
            var file = fileService.GetById(fileId);
            //bool result = fileService.DeleteFile(fileId, userId);
            //Assert.AreEqual(true, result);
        }
    }
}
