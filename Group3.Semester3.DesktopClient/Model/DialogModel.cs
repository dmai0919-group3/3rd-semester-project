using System;
using System.Collections.Generic;
using System.Text;

namespace Group3.Semester3.DesktopClient.Model
{
    public class NotificationMessage
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public string ButtonText { get; set; }
        public object ButtonCommand { get; set; }

        public bool Success = false;
        public string SuccessStatusMessage = "An unexpected error has occurred";

        public NotificationMessage()
        {
            ButtonText = "OK";
        }
    }

    public class ErrorNotificationMessage : NotificationMessage
    {
        public ErrorNotificationMessage()
        {
            Title = "Error";
        }
    }

    public class InfoNotificationMessage : NotificationMessage
    {
        public InfoNotificationMessage()
        {
            Title = "Info";
        }
    }

    public class TextPrompt : NotificationMessage
    {
        public string Text { get; set; }
    }

    public class YesNoPrompt : NotificationMessage
    {
        public string CancelButtonText { get; set; }

        public object CancelButtonCommand { get; set; }

        public YesNoPrompt()
        {
            CancelButtonText = "Cancel";
        }
    }
}
