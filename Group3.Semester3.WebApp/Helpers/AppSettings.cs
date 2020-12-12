using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string FormSecret { get; set; }
        public string AzureDefaultContainer { get; set; }
        public string AzureStorageAccount { get; set; }
        public string AzureAccountKey { get; set; }
        public string SendGridAPIKey { get; set; }
        public string EmailSenderName { get; set; }
        public string EmailSenderAddress { get; set; }
    }
}
