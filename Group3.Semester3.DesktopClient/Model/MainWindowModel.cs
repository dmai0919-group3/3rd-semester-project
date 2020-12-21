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
    public class UploadPopup : NotificationMessage
    {
        public string[] Files { get; set; }
    }

    public class MainWindowModel
    {
        public class FileEntityWrapper
        {
            public FileEntity FileEntity { get; set; }
            public BitmapSource Icon { get; set; }
            public bool Selected { get; set; }
            public bool Dummy { get; set; }
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
        public ObservableCollection<FileEntityWrapper> FileViewList { get; } = new ObservableCollection<FileEntityWrapper>();
        public ObservableCollection<GroupWrapper> GroupList { get; } = new ObservableCollection<GroupWrapper>();
        public Group SelectedGroup { get; set; } = new Group { Id = Guid.Empty };

        public List<DownloadTask> Tasks = new List<DownloadTask>();
    }
}