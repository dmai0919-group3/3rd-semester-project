using System;
using System.Collections.Generic;
using System.Text;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Models.Users;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Group3.Semester3.WebAppTests
{
    class UserServiceTests
    {
        IUserService userService;
        const string email = "marek@gmail.com";
        
        [SetUp]
        public void Setup()
        {
            userService = Helper.GetUserService();
        }

        [Test, Order(0)]
        public void Test1()
        {
            if(userService == null) {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test, Order(1)]
        public void TestRegister()
        {
            RegisterModel registerModel = new RegisterModel()
            {
                Email = email,
                Name = "marek",
                Password = "123456"
            };
            try {
            var result = userService.Register(registerModel);
            if(result != null)
            {
                Assert.AreEqual(registerModel.Email, result.Email);
                Assert.AreEqual(registerModel.Name, result.Name);
            }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [Test,Order(2)]
        public void TestLogin()
        {
            var model = new AuthenticateModel()
            {
                Email = email,
                Password = "123456"
            };

            try
            {
                var result = userService.Login(model);
                Assert.AreEqual(model.Email, result.Email);
            }
            catch(Exception Exception)
            {
                Assert.Fail(Exception.Message);
            }

        }
        [Test, Order(3)]
        public void TestDelete()
        {
            var user = userService.GetByEmail(email);
            bool result = userService.Delete(user.Id);
            Assert.AreEqual(true, result);
        }
    }
}