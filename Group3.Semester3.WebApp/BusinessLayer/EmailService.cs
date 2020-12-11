using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Models.Users;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asyncronously
        /// </summary>
        /// <param name="user">The UserModel object of the receipent user</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="contentPlainText">The content of the email in plain text format</param>
        /// <param name="contentHtmlContent">The content of the email in HTML format</param>
        /// <returns>The Response from the email request.</returns>
        Task<Response> SendEmail(UserModel user, string subject, string contentPlainText = "", string contentHtml = "");
    }

    public class EmailService : IEmailService
    {
        protected readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration.GetSection("AppSettings").Get<AppSettings>().SendGridAPIKey;
            _senderEmail = configuration.GetSection("AppSettings").Get<AppSettings>().EmailSenderAddress;
            _senderName = configuration.GetSection("AppSettings").Get<AppSettings>().EmailSenderName;
        }

        /// <summary>
        /// Sends an email asyncronously
        /// </summary>
        /// <param name="user">The UserModel object of the receipent user</param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="contentPlainText">The content of the email in plain text format</param>
        /// <param name="contentHtmlContent">The content of the email in HTML format</param>
        /// <returns>The Response from the email request.</returns>
        public async Task<Response> SendEmail(UserModel user, string subject, string contentPlainText = "", string contentHtml = "")
        {
            SendGridClient Client = new SendGridClient(_apiKey);

            EmailAddress From = new EmailAddress(_senderEmail, _senderName);
            EmailAddress To = new EmailAddress(user.Email, user.Name);

            SendGridMessage Message = MailHelper.CreateSingleEmail(From, To, subject, contentPlainText, contentHtml);

            return await Client.SendEmailAsync(Message);
        }
    }
}
