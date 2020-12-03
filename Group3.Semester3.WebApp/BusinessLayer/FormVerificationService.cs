using Group3.Semester3.WebApp.Helpers;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Form;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    public interface IFormVerificationService
    {
        public FormVerification GetVerifiedForm();

        public void VerifyForm(FormVerification verification);
    }

    public class FormVerificationService : IFormVerificationService
    {

        private readonly AppSettings _appSettings;

        public FormVerificationService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public FormVerification GetVerifiedForm()
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var verification = new FormVerification { Timestamp = milliseconds };

            var hash = "";

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                var ms = Encoding.ASCII.GetBytes(milliseconds + "");
                hmac.Key = Encoding.ASCII.GetBytes(_appSettings.FormSecret);
                hash = BitConverter.ToString(hmac.ComputeHash(ms));
            }

            verification.Token = hash;

            return verification;
        }

        public void VerifyForm(FormVerification verification)
        {
            long milliseconds = verification.Timestamp;

            var hash = "";

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                var ms = Encoding.ASCII.GetBytes(milliseconds + "");
                hmac.Key = Encoding.ASCII.GetBytes(_appSettings.FormSecret);
                hash = BitConverter.ToString(hmac.ComputeHash(ms));
            }

            if (verification.Token != hash)
            {
                throw new ValidationException("Invalid form request");
            }
        }
    }
}
