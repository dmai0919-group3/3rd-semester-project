using Group3.Semester3.WebApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Group3.Semester3.DesktopClient.Model
{
    public class MainWindowModel
    {
        public class FileEntityWrapper
        {
            public FileEntity FileEntity { get; set; }
            public BitmapSource Icon { get; set; }
            public bool Selected { get; set; }
        }
        public class DownloadTask
        {
            public delegate void AbortHandler();
            public AbortHandler Abort;
            public long Size;
            public float ProgressPercentage;
            public bool Completed;
            public string Message;
        }

        public List<DownloadTask> Tasks = new List<DownloadTask>();
        public List<FileEntityWrapper> FileViewList = new List<FileEntityWrapper>();

        public MainWindowModel()
        {
            
        }
    }
}