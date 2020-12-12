using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Models.Users;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Group3.Semester3.WebAppTests
{
    public class EmailSendingTest
    {
        private IEmailService _emailService;
        private IConfiguration _configuration;
        private UserModel user;

        [SetUp]
        public void Setup()
        {
            _configuration = Helper.ConfigurationRoot();
            _emailService = Helper.GetEmailService();
            user = new UserModel()
            {
                Name = "Test User",
                Email = "testing@ogo-file.space",
                //Activated = true,
                Id = Guid.Empty
            };
        }

        [Test]
        public async Task TestSendEmail()
        {

            string plainText = "This is a test email sent using the SendGrid NuGet package in C#.\nPlease ignore it.";
            string html = "<h1>This is a test email sent using the SendGrid NuGet package in C#.</h1><br><strong>Please ignore it.</strong>";
            var response = await _emailService.SendEmail(user, "Test Email", plainText, html);
            Console.WriteLine(await response.Body.ReadAsStringAsync());
            Assert.Pass();
        }
    }
}
