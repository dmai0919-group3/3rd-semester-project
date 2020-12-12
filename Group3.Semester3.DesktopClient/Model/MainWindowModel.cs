using Group3.Semester3.WebApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Group3.Semester3.DesktopClient.Model
{
    public class NotificationMessage
    {
        private string message = string.Empty;
        private string title = string.Empty;

        public string Message { get => message; set => message = value; }
        public string Title { get => title; set => title = value; }
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

    public class MainWindowModel
    {
        public class FileEntityWrapper
        {
            public FileEntity FileEntity { get; set; }
            public BitmapSource Icon { get; set; }
            public bool Selected { get; set; }
        }

        public class GroupWrapper : INotifyPropertyChanged
        {
            public Group Group { get; set; }
            public bool Selected
            {
                get
                {
                    return selectedValue;
                }

                set
                {
                    if (value != selectedValue)
                    {
                        this.selectedValue = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            private bool selectedValue;

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class DownloadTask
        {
            public delegate void AbortHandler();
            public AbortHandler Abort;
            public long Size;
            public float ProgressPercentage;
            public string Message;
        }

        public FileEntityWrapper SelectedFile { get; set; }

        public List<DownloadTask> Tasks = new List<DownloadTask>();
        public ObservableCollection<FileEntityWrapper> FileViewList { get; } = new ObservableCollection<FileEntityWrapper>();
        public ObservableCollection<GroupWrapper> GroupList { get; } = new ObservableCollection<GroupWrapper>();

        public MainWindowModel()
        {
            
        }
    }
}