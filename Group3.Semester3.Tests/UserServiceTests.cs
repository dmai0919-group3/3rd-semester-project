using System;
using System.Collections.Generic;
using System.Text;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Group3.Semester3.WebAppTests
{
    class UserServiceTests
    {
        IUserService userService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddTransient<IUserService, UserService>();

            var serviceProvider = services.BuildServiceProvider();

            userService = serviceProvider.GetService<IUserService>();
        }

        [Test]
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
        
    }
}
